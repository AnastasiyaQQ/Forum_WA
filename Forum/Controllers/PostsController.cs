using Forum.Dtos.Common;
using Forum.Dtos.Posts;
using Forum.Services.Interfaces;
using Forum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

//  Тип элемента - Класс Контроллера API 
// Назначение типа - Обрабатывают HTTP-запросы, вызывают сервисы для бизнес-логики, формируют HTTP-ответы.

//  Назначение элемента - Контроллер Записей (/api/Posts) 
// Отвечает за операции CRUD (создание, чтение, обновление, удаление) для записей (постов) форума.
// Большинство методов требуют аутентификации ([Authorize]), кроме публичного чтения.
// Использует IPostService для выполнения основной логики.

namespace Forum.Controllers
{
    //  Метод Действия - GetPosts 
    // Назначение - Получает список записей с пагинацией и поиском. Доступен всем ([AllowAnonymous]).
    // Выход - OkObjectResult с PagedResult<PostDto>.

    //  Метод Действия - GetPost 
    // Маршрут - GET /api/Posts/{id}
    // Назначение - Получает детальную информацию об одной записи. Доступен всем ([AllowAnonymous]).
    // Выход - OkObjectResult с PostDetailsDto или NotFoundResult.

    //  Метод Действия - CreatePost 
    // Маршрут - POST /api/Posts
    // Назначение - Создает новую запись. Требует роли "User" или "Admin" ([Authorize(Roles = ...)]).
    // Вход - CreatePostDto (в теле запроса).
    // Выход - CreatedAtActionResult с PostDetailsDto или BadRequest/Unauthorized.

    //  Метод Действия - UpdatePost 
    // Маршрут - PUT /api/Posts/{id}
    // Назначение - Обновляет существующую запись. Требует роли "User" или "Admin". Сервис проверяет авторство.
    // Вход - UpdatePostDto (в теле запроса), id (в маршруте).
    // Выход - NoContentResult, NotFoundResult, ForbidResult или BadRequest.

    //  Метод Действия - DeletePost 
    // Маршрут - DELETE /api/Posts/{id}
    // Назначение - Удаляет (перемещает в удаленные) запись. Требует роли "User" или "Admin". Сервис проверяет авторство/роль.
    // Вход - id (в маршруте).
    // Выход - OkObjectResult, NotFoundResult, ForbidResult или StatusCode 500.

    //  Метод Действия - GetMyPosts 
    // Назначение - Получает список записей текущего аутентифицированного пользователя. Требует роли "User" или "Admin".
    // Выход - OkObjectResult с PagedResult<PostDto>.

    [ApiController]
    [Route("api/[controller]")]
    // Требует аутентификации для всех методов этого контроллера по умолчанию
    [Authorize] 
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet]
        [AllowAnonymous]
        // Главная страница доступна всем

        public async Task<ActionResult<PagedResult<PostDto>>> GetPosts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5,
            [FromQuery] string searchTerm = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 5;
            // Ограничение размера страницы
            if (pageSize > 50) pageSize = 50; 

            var result = await _postService.GetPostsAsync(pageNumber, pageSize, searchTerm);
            return Ok(result);
        }

        // Доб имя для CreatedAtAction
        [HttpGet("{id}", Name = "GetPost")]
        // Просмотр записи доступен всем
        [AllowAnonymous] 
        public async Task<IActionResult> GetPost(int id)
        {
            var result = await _postService.GetPostByIdAsync(id);
            // ActionResult содержит Ok или NotFound
            return result.Result;
        }


        [HttpPost]
        // Только авторизованные пользователи и админы
        [Authorize(Roles = "User,Admin")] 
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto createPostDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _postService.CreatePostAsync(createPostDto, User);
            return result.Result;
            // Возвращает CreatedAtAction или ошибку
        }


        [HttpPut("{id}")]
        // Только автор или админ
        [Authorize(Roles = "User,Admin")] 
        public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdatePostDto updatePostDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _postService.UpdatePostAsync(id, updatePostDto, User);
            return result; 
            // Возвращает NoContent, NotFound, Forbid
        }


        [HttpDelete("{id}")]
        // Только автор или админ
        [Authorize(Roles = "User,Admin")] 
        public async Task<IActionResult> DeletePost(int id)
        {
            var result = await _postService.DeletePostAsync(id, User);
            return result; 
            // Возвращает Ok(message), NotFound, Forbid, 500
        }

        // Отдельный маршрут для "моих записей"
        [HttpGet("my")]
        // Доступно только авторизованным
        [Authorize(Roles = "User,Admin")] 
        public async Task<ActionResult<PagedResult<PostDto>>> GetMyPosts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5,
            [FromQuery] string searchTerm = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 5;
            if (pageSize > 50) pageSize = 50;

            var result = await _postService.GetMyPostsAsync(pageNumber, pageSize, searchTerm, User);
            return Ok(result);
        }
    }
}
