using Forum.Dtos.Auth;
using Forum.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

//  Тип элемента - Класс Контроллера API 
// Назначение типа - Обрабатывают HTTP-запросы, вызывают сервисы для бизнес-логики, формируют HTTP-ответы.

//  Назначение элемента - Контроллер Аутентификации (/api/Auth) 
// Отвечает за обработку запросов входа (POST /login) и регистрации (POST /register) пользователей.
// Использует IAuthService для выполнения основной логики.
// Методы Login и Register доступны анонимно ([AllowAnonymous]).

namespace Forum.Controllers
{
    //  Метод Действия - Login 
    // Маршрут - POST /api/Auth/login
    // Назначение - Аутентифицирует пользователя по логину и паролю.
    // Вход - LoginDto (в теле запроса).
    // Выход - OkObjectResult с TokenDto при успехе, UnauthorizedObjectResult или BadRequest при ошибке.

    //  Метод Действия - Register 
    // Маршрут - POST /api/Auth/register
    // Назначение - Регистрирует нового пользователя.
    // Вход - RegisterDto (в теле запроса).
    // Выход - OkObjectResult при успехе, BadRequestObjectResult при ошибке.

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous] 
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _authService.LoginAsync(loginDto);
            return result.Result;
            // Возвращает внутренний IActionResult
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _authService.RegisterAsync(registerDto);
            return result;
            // Возвращает Ok или BadRequest
        }
    }
}
