using Forum.Dtos.Comments;
using Forum.Dtos.Common;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
// Тип элемента - Интерфейс Сервиса 
// Назначение типа: Интерфейсы определяют набор методов, которые должен реализовать класс сервиса.
//  Назначение элемента - Интерфейс Сервиса Комментариев 
// Определяет операции CRUD для комментариев, получение комментариев для конкретной записи
// и получение комментариев текущего пользователя, с пагинацией и поиском.

namespace Forum.Services.Interfaces
{
    //  Метод - GetCommentsByPostIdAsync 
    // Назначение - Определяет метод для получения комментариев к конкретной записи с пагинацией и поиском.
    // Что делает - Реализация найдет пост по ID, получит страницу его комментариев из БД,
    // отсортированных по дате, опционально отфильтрованных по строке поиска, и вернет их в виде CommentDto.
    // Параметры -
    // postId - ID записи, комментарии к которой запрашиваются.
    // pageNumber - номер страницы.
    // pageSize - количество комментариев на странице.
    // searchTerm - строка для поиска в содержании комментария.
    // Возвращает - Task<PagedResult<CommentDto>> - Асинхронная операция, возвращающая объект с результатами пагинации для комментариев к данной записи.

    //  Метод - CreateCommentAsync 
    // Назначение - Определяет метод для создания нового комментария к записи.
    // Что делает - Реализация проверит существование записи по postId, создаст новый комментарий в БД
    // с данными из DTO, установив текущего пользователя (из ClaimsPrincipal) как автора и текущую дату.
    // Параметры -
    // postId - ID записи, к которой добавляется комментарий.
    // createCommentDto - объект с содержанием нового комментария.
    // userPrincipal - информация о текущем пользователе (авторе комментария).
    // Возвращает - Task<ActionResult<CommentDto>> - Асинхронная операция. Вернет CreatedAtAction с созданным CommentDto в случае успеха, NotFound если пост не найден, или Unauthorized.

    //  Метод - UpdateCommentAsync 
    // Назначение - Определяет метод для обновления существующего комментария.
    // Что делает - Реализация найдет комментарий по ID, проверит права на редактирование (автор или админ),
    // обновит содержание комментария данными из DTO и сохранит изменения в БД.
    // Параметры -
    // commentId - ID обновляемого комментария.
    // updateCommentDto - объект с новым содержанием комментария.
    // userPrincipal - информация о текущем пользователе для проверки прав.
    // Возвращает - Task<IActionResult> - Асинхронная операция. Вернет NoContent в случае успеха, NotFound если комментарий не найден, или Forbid если нет прав.

    //  Метод - DeleteCommentAsync 
    // Назначение - Определяет метод для удаления комментария (перемещения в "удаленные").
    // Что делает - Реализация найдет комментарий по ID, проверит права на удаление (автор или админ),
    // переместит комментарий в таблицу "Удаленный_комментарий" в рамках транзакции,
    // а затем удалит оригинальный комментарий из активной таблицы.
    // Параметры -
    // commentId - ID удаляемого комментария.
    // userPrincipal - информация о текущем пользователе для проверки прав.
    // Возвращает - Task<IActionResult> - Асинхронная операция. Вернет Ok с сообщением или NoContent в случае успеха, NotFound, Forbid, или StatusCode 500.

    //  Метод - GetMyCommentsAsync 
    // Назначение - Определяет метод для получения комментариев, оставленных текущим пользователем, с пагинацией и поиском.
    // Что делает - Реализация получит из БД страницу комментариев, отфильтрованных по ID текущего пользователя,
    // отсортированных по дате, опционально отфильтрованных по строке поиска, и вернет их в виде CommentDto.
    // Параметры -
    // pageNumber - номер страницы.
    // pageSize - количество комментариев на странице.
    // searchTerm - строка для поиска.
    // userPrincipal - информация о текущем пользователе для фильтрации.
    // Возвращает - Task<PagedResult<CommentDto>> - Асинхронная операция, возвращающая объект с результатами пагинации для комментариев текущего пользователя.

    public interface ICommentService
    {
        Task<PagedResult<CommentDto>> GetCommentsByPostIdAsync(int postId, int pageNumber, int pageSize, string searchTerm);
        Task<ActionResult<CommentDto>> CreateCommentAsync(int postId, CreateCommentDto createCommentDto, ClaimsPrincipal userPrincipal);
        Task<IActionResult> UpdateCommentAsync(int commentId, UpdateCommentDto updateCommentDto, ClaimsPrincipal userPrincipal);
        Task<IActionResult> DeleteCommentAsync(int commentId, ClaimsPrincipal userPrincipal);
        Task<PagedResult<CommentDto>> GetMyCommentsAsync(int pageNumber, int pageSize, string searchTerm, ClaimsPrincipal userPrincipal);
    }
}
