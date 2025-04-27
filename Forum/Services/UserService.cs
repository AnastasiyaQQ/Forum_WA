using Forum.Data;
using Forum.Dtos.Users;
using Forum.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
//  Тип элемента - Класс Сервиса 
// Назначение типа - Реализует конкретный алгоритм или логику, определенную интерфейсом сервиса.
//  Назначение элемента - Сервис Пользователей 
// Реализует интерфейс IUserService. Предоставляет методы для получения ID текущего пользователя
// из его ClaimsPrincipal и для получения информации о пользователе (DTO) по его ID из базы данных.

namespace Forum.Services
{
    //  Поле - _context 
    // Назначение - Контекст базы данных Entity Framework Core.
    // Что делает - Предоставляет доступ к таблице Users и связанным таблицам (Roles) для чтения данных.
    // Внедряется через конструктор.

    //  Конструктор - UserService 
    // Назначение - Инициализирует сервис пользователей.
    // Что делает - Получает экземпляр ForumDbContext и сохраняет его.

    //  Метод - GetCurrentUserId 
    // Назначение - Получает ID текущего аутентифицированного пользователя из ClaimsPrincipal.
    // Что делает -
    // 1. Ищет Claim (утверждение) с типом NameIdentifier (стандартный тип для ID пользователя в JWT).
    // 2. Если Claim найден, пытается преобразовать его значение (Value) в целое число (int).
    // 3. Если преобразование успешно, возвращает ID пользователя.
    // 4. В противном случае (Claim не найден ) возвращает null.
    // Параметры - userPrincipal - объект ClaimsPrincipal текущего пользователя.
    // Возвращает - int? - ID пользователя или null.

    //  Метод - GetUserDtoByIdAsync 
    // Назначение - Асинхронно получает данные пользователя в формате DTO по его ID.
    // Что делает -
    // 1. Выполняет запрос к базе данных (_context.Users).
    // 2. Включает связанную сущность Role с помощью Include(), чтобы получить доступ к имени роли.
    // 3. Фильтрует пользователей по userId с помощью Where().
    // 4. Проецирует найденного пользователя (если есть) в UserDto с помощью Select(), выбирая нужные поля (Id, Username, Role.Name).
    // 5. Выполняет запрос асинхронно с помощью FirstOrDefaultAsync(), который вернет UserDto или null, если пользователь не найден.
    // Параметры - userId - ID пользователя, которого нужно найти.
    // Возвращает - Task<UserDto?> - Задача, результатом которой будет UserDto или null.

    public class UserService : IUserService
    {
        private readonly ForumDbContext _context;

        public UserService(ForumDbContext context)
        {
            _context = context;
        }

        public int? GetCurrentUserId(ClaimsPrincipal userPrincipal)
        {
            var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }

        public async Task<UserDto> GetUserDtoByIdAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Role) 
                .Where(u => u.Id == userId)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Role = u.Role.Name 
                })
                .FirstOrDefaultAsync();

            return user;
            // Может вернуть null, если пользователь не найден
        }
    }
}
