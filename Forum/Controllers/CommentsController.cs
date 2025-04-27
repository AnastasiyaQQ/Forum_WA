using Forum.Dtos.Comments;
using Forum.Dtos.Common;
using Forum.Services.Interfaces;
using Forum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

//  Тип элемента - Класс Контроллера API 
// Назначение типа - Обрабатывают HTTP-запросы, вызывают сервисы для бизнес-логики, формируют HTTP-ответы.

//  Назначение элемента - Контроллер Комментариев 
// Отвечает за операции CRUD для комментариев к записям.
// Маршруты организованы относительно записей (для получения/создания) или ID комментариев (для обновления/удаления).
// Требует аутентификации для большинства действий, кроме чтения комментариев.
// Использует ICommentService для выполнения основной логики.

namespace Forum.Controllers
{
    //  Метод Действия - GetCommentsForPost 
    // Назначение - Получает список комментариев для конкретной записи с пагинацией и поиском. Доступен всем ([AllowAnonymous]).
    // Выход - OkObjectResult с PagedResult<CommentDto>.

    //  Метод Действия - CreateComment 
    // Маршрут - POST /api/posts/{postId}/comments
    // Назначение - Создает новый комментарий к записи. Требует роли "User" или "Admin".
    // Вход - CreateCommentDto (в теле запроса), postId (в маршруте).
    // Выход - CreatedAtActionResult с CommentDto или BadRequest/NotFound/Unauthorized.

    //  Метод Действия - UpdateComment 
    // Маршрут - PUT /api/comments/{id}
    // Назначение - Обновляет существующий комментарий. Требует роли "User" или "Admin". Сервис проверяет авторство.
    // Вход - UpdateCommentDto (в теле запроса), id (в маршруте).
    // Выход - NoContentResult, NotFoundResult, ForbidResult или BadRequest.

    //  Метод Действия - DeleteComment 
    // Маршрут - DELETE /api/comments/{id}
    // Назначение - Удаляет (перемещает в удаленные) комментарий. Требует роли "User" или "Admin". Сервис проверяет авторство/роль.
    // Вход - id (в маршруте).
    // Выход - OkObjectResult, NotFoundResult, ForbidResult или StatusCode 500.

    // Метод Действия - GetMyComments 
    // Назначение - Получает список комментариев текущего пользователя. Требует роли "User" или "Admin".
    // Выход - OkObjectResult с PagedResult<CommentDto>.

    [ApiController]
    [Route("api/")] 
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet("posts/{postId}/comments", Name = "GetCommentsForPost")]
        [AllowAnonymous] 
        // Комментарии могут смотреть все
        public async Task<ActionResult<PagedResult<CommentDto>>> GetCommentsForPost(
            int postId,
            [FromQuery] int pageNumber = 1,
            // Комментариев может быть больше на странице
            [FromQuery] int pageSize = 10, 
            [FromQuery] string searchTerm = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            // Ограничение
            if (pageSize > 100) pageSize = 100; 

            var result = await _commentService.GetCommentsByPostIdAsync(postId, pageNumber, pageSize, searchTerm);
            return Ok(result);
        }


        [HttpPost("posts/{postId}/comments")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> CreateComment(int postId, [FromBody] CreateCommentDto createCommentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _commentService.CreateCommentAsync(postId, createCommentDto, User);
            return result.Result; 
            // Возвращает CreatedAtAction или ошибку
        }

        [HttpPut("comments/{id}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentDto updateCommentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _commentService.UpdateCommentAsync(id, updateCommentDto, User);
            return result; 
            // NoContent, NotFound, Forbid
        }

        [HttpDelete("comments/{id}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var result = await _commentService.DeleteCommentAsync(id, User);
            return result; 
            // Ok(message), NotFound, Forbid, 500
        }

        [HttpGet("comments/my")]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<PagedResult<CommentDto>>> GetMyComments(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string searchTerm = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var result = await _commentService.GetMyCommentsAsync(pageNumber, pageSize, searchTerm, User);
            return Ok(result);
        }
    }
}
