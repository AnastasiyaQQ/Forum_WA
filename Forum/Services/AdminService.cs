using Forum.Data;
using Forum.Dtos.Comments;
using Forum.Dtos.Common;
using Forum.Dtos.Posts;
using Forum.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
//  Тип элемента - Класс Сервиса 
// Назначение типа - Реализует конкретный алгоритм или логику, определенную интерфейсом сервиса.
//  Назначение элемента - Сервис Администрирования 
// Реализует интерфейс IAdminService. Отвечает за получение списков удаленных записей и комментариев,
// получение деталей удаленной записи, а также за получение полного списка всех активных комментариев.

namespace Forum.Services
{
    //  Поле - _context 
    // Назначение - Контекст базы данных.
    // Что делает - Предоставляет доступ к таблицам DeletedPosts, DeletedComments, Users, Comments.
    // Внедряется через конструктор.

    //  Конструктор - AdminService 
    // Назначение - Инициализирует сервис администрирования.
    // Что делает - Получает зависимость ForumDbContext через DI.

    //  Метод - GetDeletedPostsAsync 
    // Назначение - Получает страницу удаленных записей.
    // Что делает - Запрашивает данные из таблицы DeletedPosts, использует GroupJoin и SelectMany (эквивалент Left Join), чтобы присоединить имя автора из таблицы Users (обрабатывая случай, если автор удален), применяет поиск и пагинацию, возвращает PagedResult<DeletedPostDto>.
    // Параметры - pageNumber, pageSize, searchTerm.
    // Возвращает - Task<PagedResult<DeletedPostDto>> - Результат пагинации.

    //  Метод - GetDeletedPostDetailsAsync 
    // Назначение - Получает детали одной удаленной записи.
    // Что делает - Ищет запись в DeletedPosts по ID. Если найдена, пытается найти имя автора по UserId (если он есть). Возвращает Ok с DeletedPostDetailsDto или NotFound.
    // Параметры - postId - ID удаленной записи.
    // Возвращает - Task<ActionResult<DeletedPostDetailsDto>> - Ok с DTO или NotFound.

    //  Метод - GetDeletedCommentsAsync 
    // Назначение - Получает страницу удаленных комментариев.
    // Что делает - Аналогично GetDeletedPostsAsync, но для таблицы DeletedComments. Запрашивает данные, присоединяет имя автора (с обработкой null), применяет поиск и пагинацию, возвращает PagedResult<DeletedCommentDto>.
    // Параметры - pageNumber, pageSize, searchTerm.
    // Возвращает - Task<PagedResult<DeletedCommentDto>> - Результат пагинации.

    //  Метод - GetAllCommentsAsync 
    // Назначение - Получает страницу ВСЕХ активных комментариев (для администратора).
    // Что делает - Запрашивает данные из основной таблицы Comments, включает автора, применяет поиск и пагинацию, возвращает PagedResult<CommentDto>. Не фильтрует по посту или автору.
    // Параметры - pageNumber, pageSize, searchTerm.
    // Возвращает - Task<PagedResult<CommentDto>> - Результат пагинации.

    public class AdminService : IAdminService
    {
        private readonly ForumDbContext _context;

        public AdminService(ForumDbContext context)
        {
            _context = context;
        }


        public async Task<PagedResult<DeletedPostDto>> GetDeletedPostsAsync(int pageNumber, int pageSize, string searchTerm)
        {
            var query = _context.DeletedPosts.AsQueryable();

            var joinedQuery = query
                .GroupJoin(_context.Users,
                          dp => dp.UserId,
                          u => u.Id,
                          (dp, users) => new { DeletedPost = dp, Users = users })
                .SelectMany(
                    x => x.Users.DefaultIfEmpty(), 
                    (x, u) => new { x.DeletedPost, User = u });


            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                joinedQuery = joinedQuery.Where(x => (x.DeletedPost.Title != null && x.DeletedPost.Title.ToLower().Contains(searchTerm)) ||
                                         (x.DeletedPost.Content != null && x.DeletedPost.Content.ToLower().Contains(searchTerm)));
            }

            var totalCount = await joinedQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var deletedPosts = await joinedQuery
                .OrderByDescending(x => x.DeletedPost.DeletedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DeletedPostDto
                {
                    PostId = x.DeletedPost.PostId,
                    Title = x.DeletedPost.Title,
                    AuthorName = x.User != null ? x.User.Username : "User Deleted or Unknown",
                    DeletedDate = x.DeletedPost.DeletedDate
                })
                .ToListAsync();

            return new PagedResult<DeletedPostDto>
            {
                Items = deletedPosts,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<ActionResult<DeletedPostDetailsDto>> GetDeletedPostDetailsAsync(int postId)
        {
            var deletedPost = await _context.DeletedPosts
                                .Where(dp => dp.PostId == postId)
                                .FirstOrDefaultAsync();

            if (deletedPost == null)
            {
                return new NotFoundResult();
            }

            string authorName = "User Deleted or Unknown";
            if (deletedPost.UserId.HasValue)
            {
                var author = await _context.Users
                                   .Where(u => u.Id == deletedPost.UserId.Value)
                                   .Select(u => u.Username)
                                   .FirstOrDefaultAsync();
                if (author != null)
                {
                    authorName = author;
                }
            }

            var dto = new DeletedPostDetailsDto
            {
                PostId = deletedPost.PostId,
                Title = deletedPost.Title,
                Content = deletedPost.Content,
                AuthorId = deletedPost.UserId,
                AuthorName = authorName,
                CreatedDate = deletedPost.CreatedDate,
                DeletedDate = deletedPost.DeletedDate
            };

            return new OkObjectResult(dto);
        }

        public async Task<PagedResult<DeletedCommentDto>> GetDeletedCommentsAsync(int pageNumber, int pageSize, string searchTerm)
        {
            var query = _context.DeletedComments.AsQueryable();

            var joinedQuery = query
                .GroupJoin(_context.Users,
                          dc => dc.UserId,
                          u => u.Id,
                          (dc, users) => new { DeletedComment = dc, Users = users })
                .SelectMany(
                    x => x.Users.DefaultIfEmpty(),
                    (x, u) => new { x.DeletedComment, User = u });


            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                joinedQuery = joinedQuery.Where(x => x.DeletedComment.Content != null && x.DeletedComment.Content.ToLower().Contains(searchTerm));
            }

            var totalCount = await joinedQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var deletedComments = await joinedQuery
                .OrderByDescending(x => x.DeletedComment.DeletedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                 .Select(x => new DeletedCommentDto
                 {
                     CommentId = x.DeletedComment.CommentId,
                     Content = x.DeletedComment.Content,
                     AuthorId = x.DeletedComment.UserId,
                     AuthorName = x.User != null ? x.User.Username : "User Deleted or Unknown",
                     CreatedDate = x.DeletedComment.CreatedDate,
                     DeletedDate = x.DeletedComment.DeletedDate,
                     PostId = x.DeletedComment.PostId
                 })
                .ToListAsync();

            return new PagedResult<DeletedCommentDto>
            {
                Items = deletedComments,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<PagedResult<CommentDto>> GetAllCommentsAsync(int pageNumber, int pageSize, string searchTerm)
        {

            var query = _context.Comments
                                .Include(c => c.User) 
                                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(c => c.Content.ToLower().Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var comments = await query
                .OrderByDescending(c => c.CreatedDate) 
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
    }
}
