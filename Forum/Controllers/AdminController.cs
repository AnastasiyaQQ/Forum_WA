using Forum.Dtos.Comments;
using Forum.Dtos.Common;
using Forum.Dtos.Posts;
using Forum.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

//  Тип элемента - Класс Контроллера API 
// Назначение типа - Обрабатывают HTTP-запросы, вызывают сервисы для бизнес-логики, формируют HTTP-ответы.

//  Назначение элемента - Контроллер Администрирования (/api/Admin) 
// Отвечает за операции, доступные только пользователям с ролью "Admin".
// Включает управление активными записями/комментариями,
// просмотр удаленных записей и комментариев.
// Все методы требуют роли "Admin" ([Authorize(Roles = "Admin")] на уровне контроллера).
// Использует IPostService, ICommentService, IAdminService.

namespace Forum.Controllers
{
    //  Метод Действия - GetAllPosts 
    // Назначение - Получает список всех активных записей с пагинацией и поиском.
    // Выход - OkObjectResult с PagedResult<PostDto>.

    //  Метод Действия - DeleteAnyPost 
    // Маршрут - DELETE /api/Admin/posts/{id}
    // Назначение - Удаляет (перемещает) любую запись (права админа проверяются сервисом).
    // Вход - id (в маршруте).
    // Выход - OkObjectResult, NotFoundResult, ForbidResult или StatusCode 500.

    //  Метод Действия - GetAllComments 
    // Назначение - Получает список ВСЕХ активных комментариев с пагинацией и поиском.
    // Выход - OkObjectResult с PagedResult<CommentDto>.

    //  Метод Действия (Удаление Комментария) - 
    // DELETE /api/comments/{id} (в CommentsController), так как CommentService уже включает проверку роли администратора.

    //  Метод Действия - GetDeletedPosts 
    // Назначение - Получает список удаленных записей с пагинацией и поиском.
    // Выход - OkObjectResult с PagedResult<DeletedPostDto>.

    //  Метод Действия - GetDeletedPostDetails 
    // Маршрут - GET /api/Admin/deleted/posts/{postId}
    // Назначение - Получает детальную информацию об одной удаленной записи.
    // Выход - OkObjectResult с DeletedPostDetailsDto или NotFoundResult.

    //  Метод Действия - GetDeletedComments 
    // Назначение - Получает список удаленных комментариев с пагинацией и поиском.
    // Выход - OkObjectResult с PagedResult<DeletedCommentDto>.

    [ApiController]
    [Route("api/[controller]")] 
    [Authorize(Roles = "Admin")] 
    // Только для администраторов
    public class AdminController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly IAdminService _adminService; 

        public AdminController(
            IPostService postService,
            ICommentService commentService,
            IAdminService adminService)
        {
            _postService = postService;
            _commentService = commentService;
            _adminService = adminService;
        }

        #region Active Posts Management (Delegating)

        [HttpGet("posts")]
        public async Task<ActionResult<PagedResult<PostDto>>> GetAllPosts(
             [FromQuery] int pageNumber = 1,
             [FromQuery] int pageSize = 5,
             [FromQuery] string searchTerm = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 5;
            if (pageSize > 50) pageSize = 50;
            // общий метод сервиса постов
            var result = await _postService.GetPostsAsync(pageNumber, pageSize, searchTerm);
            return Ok(result);
        }

        [HttpDelete("posts/{id}")]
        public async Task<IActionResult> DeleteAnyPost(int id)
        {
            var result = await _postService.DeletePostAsync(id, User);
            return result;
        }

        #endregion

        #region Active Comments Management

        [HttpGet("comments")] 
        public async Task<ActionResult<PagedResult<CommentDto>>> GetAllComments(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string searchTerm = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var result = await _adminService.GetAllCommentsAsync(pageNumber, pageSize, searchTerm);
            return Ok(result); 
        }
      
        #endregion

        #region Deleted Items Management (Без изменений)

        [HttpGet("deleted/posts")]
        public async Task<ActionResult<PagedResult<DeletedPostDto>>> GetDeletedPosts(
             [FromQuery] int pageNumber = 1,
             [FromQuery] int pageSize = 5,
             [FromQuery] string searchTerm = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 5;
            if (pageSize > 50) pageSize = 50;

            var result = await _adminService.GetDeletedPostsAsync(pageNumber, pageSize, searchTerm);
            return Ok(result);
        }

        [HttpGet("deleted/posts/{postId}")]
        public async Task<IActionResult> GetDeletedPostDetails(int postId)
        {
            var result = await _adminService.GetDeletedPostDetailsAsync(postId);
            return result.Result; 
        }

        [HttpGet("deleted/comments")]
        public async Task<ActionResult<PagedResult<DeletedCommentDto>>> GetDeletedComments(
             [FromQuery] int pageNumber = 1,
             [FromQuery] int pageSize = 10,
             [FromQuery] string searchTerm = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var result = await _adminService.GetDeletedCommentsAsync(pageNumber, pageSize, searchTerm);
            return Ok(result);
        }

        #endregion
    }
}