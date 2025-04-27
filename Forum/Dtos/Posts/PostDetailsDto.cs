using System;
//  Тип элемента - DTO (Data Transfer Object) 
// Назначение типа - DTO используются для передачи данных между слоями. 
// Классы, представляющие данные, получаемые от сервера и отправляемые на сервер.

//  Назначение элемента - Детальная информация о записи 
// Используется для отображения полной информации об одной конкретной записи.
// Включает содержание записи, в отличие от PostDto.

namespace Forum.Dtos.Posts
{
    public class PostDetailsDto
    {
        //  Свойство - Id 
        // Назначение - Уникальный идентификатор записи.
        // Тип данных - int

        //  Свойство - Title 
        // Назначение - Заголовок записи.
        // Тип данных - string

        //  Свойство - Content 
        // Назначение - Полное содержание (текст) записи.
        // Тип данных - string

        //  Свойство - AuthorName 
        // Назначение - Имя автора записи.
        // Тип данных - string

        //  Свойство - AuthorId 
        // Назначение - ID автора записи (может быть полезно на клиенте для проверок).
        // Тип данных - int

        //  Свойство - CreatedDate 
        // Назначение - Дата создания записи.
        // Тип данных - DateTime

        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string AuthorName { get; set; }
        public int AuthorId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
