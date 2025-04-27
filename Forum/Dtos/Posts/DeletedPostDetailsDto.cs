using System;
//  Тип элемента - DTO (Data Transfer Object) 
// Назначение типа - DTO используются для передачи данных между слоями. 
// Классы, представляющие данные, получаемые от сервера и отправляемые на сервер.

//  Назначение элемента - Детальная информация об удаленной записи 
// Используется для отображения полной информации об одной конкретной удаленной записи на администраторской странице просмотра.

namespace Forum.Dtos.Posts
{
    //  Свойство - PostId 
    // Назначение - Оригинальный ID записи.
    // Тип данных - int

    //  Свойство - Title 
    // Назначение - Оригинальный заголовок записи.
    // Тип данных - string

    //  Свойство - Content 
    // Назначение - Оригинальное содержание (текст) записи.
    // Тип данных - string

    //  Свойство - AuthorName 
    // Назначение - Имя автора оригинальной записи. Может быть null или "User Deleted or Unknown".
    // Тип данных - string? (или string)

    //  Свойство - AuthorId 
    // Назначение - Оригинальный ID автора записи. Может быть null, если информация недоступна.
    // Тип данных - int? (nullable int)

    //  Свойство - CreatedDate 
    // Назначение - Оригинальная дата создания записи. Может быть null.
    // Тип данных - DateTime? (nullable DateTime)

    //  Свойство - DeletedDate 
    // Назначение - Дата удаления записи.
    // Тип данных - DateTime

    public class DeletedPostDetailsDto
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string AuthorName { get; set; } 
        public int? AuthorId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime DeletedDate { get; set; }
    }
}
