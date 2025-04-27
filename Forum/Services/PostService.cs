using Forum.Data;
using Forum.Dtos.Common;
using Forum.Dtos.Posts;
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
//  Назначение элемента - Сервис Записей 
// Реализует интерфейс IPostService. Отвечает за создание, чтение, обновление, удаление записей,
// получение списков записей (всех или пользователя) с пагинацией и поиском,
// а также за обработку удаления связанных комментариев при удалении записи.

namespace Forum.Services
{
    //  Поле - _context 
    // Назначение - Контекст базы данных.
    // Что делает - Предоставляет доступ ко всем таблицам БД (Posts, Users, Comments, DeletedPosts, DeletedComments).
    // Внедряется через конструктор.

    //  Поле - _userService 
    // Назначение - Сервис для работы с пользователями.
    // Что делает - Используется для получения ID текущего пользователя из ClaimsPrincipal.
    // Внедряется через конструктор.

    //  Конструктор - PostService 
    // Назначение - Инициализирует сервис записей.
    // Что делает - Получает зависимости ForumDbContext и IUserService.

    //  Метод - GetPostsAsync 
    // Назначение - Получает страницу записей для главной страницы (или поиска по всем).
    // Что делает - Выполняет запрос к БД для получения записей с пагинацией и опциональным поиском по заголовку/содержанию, включает имя автора, возвращает PagedResult<PostDto>.
    // Параметры - pageNumber, pageSize, searchTerm.
    // Возвращает - Task<PagedResult<PostDto>> - Результат пагинации.

    //  Метод - GetPostByIdAsync 
    // Назначение - Получает детальную информацию об одной записи.
    // Что делает - Ищет запись по ID, включает автора, проецирует результат в PostDetailsDto.
    // Параметры - postId - ID записи.
    // Возвращает - Task<ActionResult<PostDetailsDto>> - Ok с DTO или NotFound.

    //  Метод - CreatePostAsync 
    // Назначение - Создает новую запись.
    // Что делает - Получает ID текущего пользователя, создает объект Post, заполняет его данными из DTO и ID автора, устанавливает дату, сохраняет в БД, возвращает CreatedAtAction с DTO созданной записи.
    // Параметры - createPostDto, userPrincipal.
    // Возвращает - Task<ActionResult<PostDetailsDto>> - CreatedAtAction с DTO или ошибка Unauthorized.

    //  Метод - UpdatePostAsync 
    // Назначение - Обновляет существующую запись.
    // Что делает - Находит запись, проверяет права доступа, обновляет поля Title и Content, сохраняет изменения. 
    // Параметры - postId, updatePostDto, userPrincipal.
    // Возвращает - Task<IActionResult> - NoContent, NotFound или Forbid.

    //  Метод - DeletePostAsync 
    // Назначение - Удаляет запись (перемещает в удаленные).
    // Что делает - Находит запись, проверяет права доступа (автор или админ). В рамках транзакции - перемещает все комментарии этой записи в DeletedComments, перемещает саму запись в DeletedPosts, удаляет оригинальные комментарии и запись из активных таблиц. Коммитит или откатывает транзакцию.
    // Параметры - postId, userPrincipal.
    // Возвращает - Task<IActionResult> - Ok с сообщением, NotFound, Forbid или StatusCode 500.

    //  Метод - GetMyPostsAsync 
    // Назначение - Получает страницу записей текущего пользователя.
    // Что делает - Выполняет запрос к БД для получения записей, отфильтрованных по ID текущего пользователя, с пагинацией и опциональным поиском, включает имя автора, возвращает PagedResult<PostDto>.
    // Параметры - pageNumber, pageSize, searchTerm, userPrincipal.
    // Возвращает - Task<PagedResult<PostDto>> - Результат пагинации для записей пользователя.

    public class PostService : IPostService
    {
        private readonly ForumDbContext _context;
        private readonly IUserService _userService;

        public PostService(ForumDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<PagedResult<PostDto>> GetPostsAsync(int pageNumber, int pageSize, string searchTerm)
        {
            // Include User для имени автора
            var query = _context.Posts.Include(p => p.User).AsQueryable(); 

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(searchTerm) || p.Content.ToLower().Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var posts = await query
                // Сортировка по дате
                .OrderByDescending(p => p.CreatedDate) 
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    // Если автора нет
                    AuthorName = p.User != null ? p.User.Username : "Unknown", 
                    CreatedDate = p.CreatedDate
                })
                .ToListAsync();

            return new PagedResult<PostDto>
            {
                Items = posts,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<ActionResult<PostDetailsDto>> GetPostByIdAsync(int postId)
        {
            var post = await _context.Posts
                .Include(p => p.User) 
                .Where(p => p.Id == postId)
                .Select(p => new PostDetailsDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    AuthorId = p.UserId,
                    AuthorName = p.User != null ? p.User.Username : "Unknown",
                    CreatedDate = p.CreatedDate
                })
                .FirstOrDefaultAsync();

            if (post == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(post);
        }

        public async Task<ActionResult<PostDetailsDto>> CreatePostAsync(CreatePostDto createPostDto, ClaimsPrincipal userPrincipal)
        {
            var userId = _userService.GetCurrentUserId(userPrincipal);
            if (userId == null)
            {
                return new UnauthorizedResult(); 
            }

            var post = new Post
            {
                Title = createPostDto.Title,
                Content = createPostDto.Content,
                UserId = userId.Value,
                // Сохр только дату
                CreatedDate = DateTime.UtcNow.Date 
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // созданный пост с именем автора для возврата
            var result = await GetPostByIdAsync(post.Id);
            if (result.Result is OkObjectResult okResult)
            {
                return new CreatedAtActionResult("GetPost", "Posts", new { id = post.Id }, okResult.Value);
            }
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        public async Task<IActionResult> UpdatePostAsync(int postId, UpdatePostDto updatePostDto, ClaimsPrincipal userPrincipal)
        {
            var userId = _userService.GetCurrentUserId(userPrincipal);
            if (userId == null) return new UnauthorizedResult();

            var post = await _context.Posts.FindAsync(postId);

            if (post == null)
            {
                return new NotFoundResult();
            }

            // Проверка прав: пользователь может редактировать только свои посты
            if (post.UserId != userId.Value)
            {
                return new ForbidResult();
            }

            post.Title = updatePostDto.Title;
            post.Content = updatePostDto.Content;
            // Дата создания не меняется при редактировании

            _context.Entry(post).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Posts.AnyAsync(p => p.Id == postId))
                {
                    return new NotFoundResult();
                }
                else
                {
                    throw; 
                }
            }

            return new NoContentResult();
        }

        public async Task<IActionResult> DeletePostAsync(int postId, ClaimsPrincipal userPrincipal)
        {
            var userId = _userService.GetCurrentUserId(userPrincipal);
            if (userId == null) return new UnauthorizedResult();

            var post = await _context.Posts
                                    // Вкл комментарии для их удаления/перемещения
                                    .Include(p => p.Comments) 
                                    .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
            {
                return new NotFoundResult();
            }

            bool isAdmin = userPrincipal.IsInRole("Admin");

            // Проверка прав: пользователь может удалять свои посты, админ - любые
            if (post.UserId != userId.Value && !isAdmin)
            {
                return new ForbidResult();
            }


            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Переместить комментарии к этому посту в Удаленные_комментарии
                var deletedComments = post.Comments.Select(c => new DeletedComment
                {
                    CommentId = c.Id,
                    PostId = c.PostId,
                    CreatedDate = c.CreatedDate,
                    UserId = c.UserId,
                    Content = c.Content,
                    DeletedDate = DateTime.UtcNow.Date
                }).ToList();

                if (deletedComments.Any())
                {
                    await _context.DeletedComments.AddRangeAsync(deletedComments);
                    // Удалить оригинальные комментарии
                    _context.Comments.RemoveRange(post.Comments); 
                }

                // 2. Переместить сам пост в Удаленные_записи
                var deletedPost = new DeletedPost
                {
                    PostId = post.Id,
                    Title = post.Title,
                    CreatedDate = post.CreatedDate,
                    UserId = post.UserId,
                    Content = post.Content,
                    DeletedDate = DateTime.UtcNow.Date
                };
                await _context.DeletedPosts.AddAsync(deletedPost);

                // 3. Удалить оригинальный пост
                _context.Posts.Remove(post);

                // 4. Сохранить все изменения 
                await _context.SaveChangesAsync();

                // 5. Подтвердить
                await transaction.CommitAsync();

                return new OkObjectResult("Post and associated comments moved to deleted items successfully."); 
            }
            catch (Exception ex) 
            {
                await transaction.RollbackAsync();
                return new StatusCodeResult(StatusCodes.Status500InternalServerError); 
            }
        }


        public async Task<PagedResult<PostDto>> GetMyPostsAsync(int pageNumber, int pageSize, string searchTerm, ClaimsPrincipal userPrincipal)
        {
            var userId = _userService.GetCurrentUserId(userPrincipal);
            if (userId == null)
            {
                // Вернуть пустой результат или ошибку
                return new PagedResult<PostDto>();
            }

            var query = _context.Posts
                                .Include(p => p.User)
                                // Фильтр по ID текущего пользователя
                                .Where(p => p.UserId == userId.Value);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(searchTerm) || p.Content.ToLower().Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var posts = await query
                .OrderByDescending(p => p.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                 .Select(p => new PostDto 
                 {
                     Id = p.Id,
                     Title = p.Title,
                     // Имя из связанной сущности
                     AuthorName = p.User.Username,
                     CreatedDate = p.CreatedDate
                 })
                .ToListAsync();

            return new PagedResult<PostDto>
            {
                Items = posts,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }
    }
}
