using Forum.Data;
using Forum.Dtos.Auth;
using Forum.Dtos.Users;
using Forum.Entities;
using Forum.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; 
using Microsoft.IdentityModel.Tokens; 
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt; 
using System.Security.Claims; 
using System.Text; 
using System.Threading.Tasks;
//  Тип элемента - Класс Сервиса 
// Назначение типа - Реализует конкретный алгоритм или логику, определенную интерфейсом сервиса.
//  Назначение элемента - Сервис Аутентификации 
// Реализует логику входа и регистрации пользователей, включая проверку паролей и генерацию JWT токенов.
// Реализует интерфейс IAuthService.

namespace Forum.Services
{
    //  Поле - _context 
    // Назначение - Контекст базы данных Entity Framework Core.
    // Что делает - Предоставляет доступ к таблицам базы данных (Users, Roles и т.д.) для чтения и записи.
    // Внедряется через конструктор.

    //  Поле - _passwordHasher 
    // Назначение - Сервис для хеширования и проверки паролей.
    // Что делает - Предоставляет методы для безопасного хеширования паролей при регистрации
    // и проверки введенного пароля с хешем из БД при входе.
    // Внедряется через конструктор.

    //  Поле - _configuration 
    // Назначение - Доступ к конфигурации приложения.
    // Что делает - Позволяет читать настройки из appsettings.json, такие как секретный ключ JWT, Issuer, Audience.
    // Внедряется через конструктор.

    //  Поле - _userService 
    // Назначение - Сервис для работы с пользователями.
    // Что делает - В данном сервисе используется только для получения ID пользователя.
    // Внедряется через конструктор.

    //  Константы - Роли 
    // Назначение - Определяют числовые ID для ролей "Admin" и "User".
    // Что делает - Используются для присвоения правильной роли при регистрации.

    //  Конструктор - AuthService 
    // Назначение - Инициализирует сервис аутентификации.
    // Что делает - Получает экземпляры зависимостей (DbContext, PasswordHasher, Configuration, UserService)
    // через механизм Dependency Injection и сохраняет их в приватных полях класса.

    //  Метод - LoginAsync 
    // Назначение - Выполняет вход пользователя.
    // Что делает - Ищет пользователя, проверяет пароль, генерирует JWT токен с ID, именем и ролью пользователя, возвращает токен и информацию о пользователе.
    // Параметры - loginDto - данные для входа.
    // Возвращает - Task<ActionResult<TokenDto>> - Результат с токеном или ошибка Unauthorized.

    //  Метод - RegisterAsync 
    // Назначение - Регистрирует нового пользователя.
    // Что делает - Проверяет уникальность имени, хеширует пароль, создает пользователя с ролью "User", сохраняет в БД, возвращает сообщение об успехе или ошибке.
    // Параметры - registerDto - данные для регистрации.
    // Возвращает - Task<ActionResult> - Результат операции (Ok или BadRequest).

    public class AuthService : IAuthService
    {
        private readonly ForumDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        // Роли по ID 
        private const int AdminRoleId = 1;
        private const int UserRoleId = 2;

        public AuthService(ForumDbContext context, IPasswordHasher passwordHasher, IConfiguration configuration, IUserService userService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _userService = userService;
        }

        public async Task<ActionResult<TokenDto>> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                                      // Важно для получения роли
                                     .Include(u => u.Role) 
                                     .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user == null || !_passwordHasher.VerifyPassword(user.PasswordHash, loginDto.Password))
            {
                return new UnauthorizedObjectResult("Invalid username or password.");
            }

            // Генерация токена
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var expires = DateTime.UtcNow.AddHours(3); 

            var claims = new List<Claim>
            {
                // ID пользователя
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), 
                // Имя пользователя
                new Claim(ClaimTypes.Name, user.Username), 
                // Роль пользователя
                new Claim(ClaimTypes.Role, user.Role.Name) 
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(securityToken);

            var userDto = new UserDto { Id = user.Id, Username = user.Username, Role = user.Role.Name };

            return new OkObjectResult(new TokenDto
            {
                AccessToken = accessToken,
                ExpiresAt = expires,
                User = userDto
            });
        }

        public async Task<ActionResult> RegisterAsync(RegisterDto registerDto)
        {
            // Проверка, существует ли пользователь с таким именем
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
            {
                return new BadRequestObjectResult("Username already exists.");
            }

            // Хэширование пароля
            var passwordHash = _passwordHasher.HashPassword(registerDto.Password);

            var newUser = new User
            {
                Username = registerDto.Username,
                PasswordHash = passwordHash,
                RoleId = UserRoleId // Все регистрирующиеся - обычные пользователи
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Не возвращаем пользователя здесь, т.к. регистрация не означает автоматический вход
            return new OkObjectResult("Registration successful. Please login.");
        }
    }
}
