using Forum.Controllers;
using Forum.Data;
using Forum.Dtos.Comments;
using Forum.Dtos.Common;
using Forum.Entities;
using Forum.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
//  Тип элемента - Класс Сервиса 
// Назначение типа - Реализует конкретный алгоритм или логику, определенную интерфейсом сервиса.
//  Назначение элемента - Сервис Комментариев 
// Реализует интерфейс ICommentService. Отвечает за создание, чтение, обновление, удаление комментариев,
// получение списков комментариев (к посту или пользователя) с пагинацией и поиском.

namespace Forum.Services
{
    //  Поле - _context 
    // Назначение - Контекст базы данных.
    // Что делает - Предоставляет доступ к таблицам Comments, Users, Posts, DeletedComments.
    // Внедряется через конструктор.

    //  Поле - _userService 
    // Назначение - Сервис пользователей.
    // Что делает - Используется для получения ID текущего пользователя.
    // Внедряется через конструктор.

    //  Конструктор - CommentService 
    // Назначение - Инициализирует сервис комментариев.
    // Что делает - Получает зависимости ForumDbContext и IUserService.

    //  Метод - GetCommentsByPostIdAsync 
    // Назначение - Получает страницу комментариев для конкретной записи.
    // Что делает - Проверяет существование поста, запрашивает комментарии к этому посту из БД с пагинацией, включает имя автора, возвращает PagedResult<CommentDto>.
    // Параметры - postId, pageNumber, pageSize, searchTerm.
    // Возвращает - Task<PagedResult<CommentDto>> - Результат пагинации.

    //  Метод - CreateCommentAsync 
    // Назначение - Создает новый комментарий к записи.
    // Что делает - Получает ID пользователя, проверяет существование поста, создает объект Comment, заполняет данными, сохраняет в БД, возвращает CreatedAtAction с DTO созданного комментария.
    // Параметры - postId, createCommentDto, userPrincipal.
    // Возвращает - Task<ActionResult<CommentDto>> - CreatedAtAction с DTO, NotFound или Unauthorized.

    //  Метод - UpdateCommentAsync 
    // Назначение - Обновляет существующий комментарий.
    // Что делает - Находит комментарий, проверяет права доступа, обновляет поле Content, сохраняет изменения. Обрабатывает DbUpdateConcurrencyException.
    // Параметры - commentId, updateCommentDto, userPrincipal.
    // Возвращает - Task<IActionResult> - NoContent, NotFound или Forbid.

    //  Метод - DeleteCommentAsync 
    // Назначение - Удаляет комментарий (перемещает в удаленные).
    // Что делает - Находит комментарий, проверяет права доступа (автор или админ). В рамках транзакции - перемещает комментарий в DeletedComments, удаляет оригинальный комментарий.
    // Параметры - commentId, userPrincipal.
    // Возвращает - Task<IActionResult> - Ok с сообщением, NotFound, Forbid или StatusCode 500.

    //  Метод - GetMyCommentsAsync 
    // Назначение - Получает страницу комментариев текущего пользователя.
    // Что делает - Выполняет запрос к БД для получения комментариев, отфильтрованных по ID текущего пользователя, с пагинацией и опциональным поиском, включает имя автора, возвращает PagedResult<CommentDto>.
    // Параметры - pageNumber, pageSize, searchTerm, userPrincipal.
    // Возвращает - Task<PagedResult<CommentDto>> - Результат пагинации для комментариев пользователя.

    public class CommentService : ICommentService
    {
        private readonly ForumDbContext _context;
        private readonly IUserService _userService;

        public CommentService(ForumDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<PagedResult<CommentDto>> GetCommentsByPostIdAsync(int postId, int pageNumber, int pageSize, string searchTerm)
        {
            // Проверка, существует ли пост
            if (!await _context.Posts.AnyAsync(p => p.Id == postId))
            {
                // Вернуть пустой результат или ошибку 
                return new PagedResult<CommentDto> { Items = new List<CommentDto>() };
            }

            var query = _context.Comments
                                // Вкл User для имени автора
                                .Include(c => c.User) 
                                .Where(c => c.PostId == postId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(c => c.Content.ToLower().Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var comments = await query
                .OrderBy(c => c.CreatedDate) 
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    AuthorId = c.UserId,
                    AuthorName = c.User != null ? c.User.Username : "Unknown",
                    CreatedDate = c.CreatedDate,
                    PostId = c.PostId
                })
                .ToListAsync();

            return new PagedResult<CommentDto>
            {
                Items = comments,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<ActionResult<CommentDto>> CreateCommentAsync(int postId, CreateCommentDto createCommentDto, ClaimsPrincipal userPrincipal)
        {
            var userId = _userService.GetCurrentUserId(userPrincipal);
            if (userId == null) return new UnauthorizedResult();

            // Проверка, существует ли пост
            if (!await _context.Posts.AnyAsync(p => p.Id == postId))
            {
                return new NotFoundObjectResult($"Post with ID {postId} not found.");
            }

            var comment = new Comment
            {
                PostId = postId,
                Content = createCommentDto.Content,
                UserId = userId.Value,
                CreatedDate = DateTime.UtcNow.Date
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Созданный комментарий для возврата
            var createdCommentDto = await _context.Comments
                .Where(c => c.Id == comment.Id)
                .Include(c => c.User)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    AuthorId = c.UserId,
                    AuthorName = c.User != null ? c.User.Username : "Unknown",
                    CreatedDate = c.CreatedDate,
                    PostId = c.PostId
                })
                .FirstOrDefaultAsync();

            return new CreatedAtActionResult(
                          actionName: "GetCommentsForPost", 
                          controllerName: "Comments",        
                          routeValues: new { postId = postId },
                          value: createdCommentDto);   

        }


        public async Task<IActionResult> UpdateCommentAsync(int commentId, UpdateCommentDto updateCommentDto, ClaimsPrincipal userPrincipal)
        {
            var userId = _userService.GetCurrentUserId(userPrincipal);
            if (userId == null) return new UnauthorizedResult();

            var comment = await _context.Comments.FindAsync(commentId);

            if (comment == null)
            {
                return new NotFoundResult();
            }

            // Проверка прав: пользователь может редактировать только свои комментарии
            if (comment.UserId != userId.Value)
            {
                return new ForbidResult();
            }

            comment.Content = updateCommentDto.Content;
            _context.Entry(comment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Comments.AnyAsync(c => c.Id == commentId)) return new NotFoundResult();
                else throw;
            }

            return new NoContentResult();
        }


        public async Task<IActionResult> DeleteCommentAsync(int commentId, ClaimsPrincipal userPrincipal)
        {
            var userId = _userService.GetCurrentUserId(userPrincipal);
            if (userId == null) return new UnauthorizedResult();

            var comment = await _context.Comments.FindAsync(commentId);

            if (comment == null)
            {
                return new NotFoundResult();
            }

            bool isAdmin = userPrincipal.IsInRole("Admin");

            // Проверка прав: пользователь может удалять свои комменты, админ - любые
            if (comment.UserId != userId.Value && !isAdmin)
            {
                return new ForbidResult();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Переместить комментарий в Удаленные_комментарии
                var deletedComment = new DeletedComment
                {
                    CommentId = comment.Id,
                    PostId = comment.PostId,
                    CreatedDate = comment.CreatedDate,
                    UserId = comment.UserId,
                    Content = comment.Content,
                    DeletedDate = DateTime.UtcNow.Date
                };
                await _context.DeletedComments.AddAsync(deletedComment);

                // 2. Удалить оригинальный комментарий
                _context.Comments.Remove(comment);

                // 3. Сохранить изменения
                await _context.SaveChangesAsync();

                // 4. Подтвердить
                await transaction.CommitAsync();

                return new OkObjectResult("Comment moved to deleted items successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Ошибки 
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

        }


        public async Task<PagedResult<CommentDto>> GetMyCommentsAsync(int pageNumber, int pageSize, string searchTerm, ClaimsPrincipal userPrincipal)
        {
            var userId = _userService.GetCurrentUserId(userPrincipal);
            if (userId == null)
            {
                // Пустой результат для неавторизованного
                return new PagedResult<CommentDto>(); 
            }

            var query = _context.Comments
                                .Include(c => c.User)
                                .Where(c => c.UserId == userId.Value);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(c => c.Content.ToLower().Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var comments = await query
                // Сортировка по дате
                .OrderByDescending(c => c.CreatedDate) 
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CommentDto 
                {
                    Id = c.Id,
                    Content = c.Content,
                    AuthorId = c.UserId,
                    AuthorName = c.User.Username,
                    CreatedDate = c.CreatedDate,
                    PostId = c.PostId
                })
                .ToListAsync();

            return new PagedResult<CommentDto>
            {
                Items = comments,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }
    }
}
