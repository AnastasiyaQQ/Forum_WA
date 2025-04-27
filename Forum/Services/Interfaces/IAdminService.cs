using Forum.Dtos.Common;
using Forum.Dtos.Posts;
using Forum.Dtos.Comments;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
// Тип элемента - Интерфейс Сервиса 
// Назначение типа: Интерфейсы определяют набор методов, которые должен реализовать класс сервиса.
//  Назначение элемента - Интерфейс Сервиса Администрирования 
// Определяет операции для управления удаленными записями и комментариями,
// а также для получения полного списка всех комментариев в системе.

namespace Forum.Services.Interfaces
{
    //  Метод - GetDeletedPostsAsync 
    // Назначение - Определяет метод для получения списка удаленных записей с пагинацией и поиском.
    // Что делает - Реализация получит страницу записей из таблицы "Удаленная_запись",
    // отсортированных по дате удаления, опционально отфильтрованных по строке поиска, и вернет их в виде DeletedPostDto.
    // Параметры - pageNumber, pageSize, searchTerm.
    // Возвращает - Task<PagedResult<DeletedPostDto>> - Результат пагинации для удаленных записей.

    //  Метод - GetDeletedPostDetailsAsync 
    // Назначение - Определяет метод для получения детальной информации об одной удаленной записи по ее ID.
    // Что делает - Реализация найдет запись в таблице "Удаленная_запись" по ID и вернет ее полные данные в виде DeletedPostDetailsDto.
    // Параметры - postId - ID запрашиваемой удаленной записи.
    // Возвращает - Task<ActionResult<DeletedPostDetailsDto>> - Ok с DeletedPostDetailsDto или NotFound.

    //  Метод - GetDeletedCommentsAsync 
    // Назначение - Определяет метод для получения списка удаленных комментариев с пагинацией и поиском.
    // Что делает - Реализация получит страницу комментариев из таблицы "Удаленный_комментарий",
    // отсортированных по дате удаления, опционально отфильтрованных по строке поиска, и вернет их в виде DeletedCommentDto.
    // Параметры - pageNumber, pageSize, searchTerm.
    // Возвращает - Task<PagedResult<DeletedCommentDto>> - Результат пагинации для удаленных комментариев.

    //  Метод - GetAllCommentsAsync 
    // Назначение - Определяет метод для получения полного списка ВСЕХ активных комментариев с пагинацией и поиском (для администратора).
    // Что делает - Реализация получит страницу комментариев из основной таблицы "Комментарий" (без фильтрации по посту или автору),
    // отсортированных по дате, опционально отфильтрованных по строке поиска, и вернет их в виде CommentDto.
    // Параметры - pageNumber, pageSize, searchTerm.
    // Возвращает - Task<PagedResult<CommentDto>> - Результат пагинации для всех комментариев.

    public interface IAdminService
    {
        Task<PagedResult<CommentDto>> GetAllCommentsAsync(int pageNumber, int pageSize, string searchTerm);

        // Методы для удаленных элементов
        Task<PagedResult<DeletedPostDto>> GetDeletedPostsAsync(int pageNumber, int pageSize, string searchTerm);
        Task<ActionResult<DeletedPostDetailsDto>> GetDeletedPostDetailsAsync(int postId);
        Task<PagedResult<DeletedCommentDto>> GetDeletedCommentsAsync(int pageNumber, int pageSize, string searchTerm);
    }
}
