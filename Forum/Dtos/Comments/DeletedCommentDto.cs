using System;
//  Тип элемента - DTO (Data Transfer Object) 
// Назначение типа - DTO используются для передачи данных между слоями. 
// Классы, представляющие данные, получаемые от сервера и отправляемые на сервер.

//  Назначение элемента - Информация об удаленном комментарии (для списков) 
// Используется для отображения удаленных комментариев в списке на соответствующей администраторской странице.

namespace Forum.Dtos.Comments
{
    //  Свойство - CommentId 
    // Назначение - Оригинальный ID комментария.
    // Тип данных - int

    //  Свойство - Content 
    // Назначение - Оригинальное содержание комментария.
    // Тип данных - string

    //  Свойство - AuthorName 
    // Назначение - Имя автора оригинального комментария. Может быть null или "User Deleted or Unknown".
    // Тип данных - string?

    //  Свойство - AuthorId 
    // Назначение - Оригинальный ID автора комментария. Может быть null.
    // Тип данных - int?

    //  Свойство - CreatedDate 
    // Назначение - Оригинальная дата создания комментария. Может быть null.
    // Тип данных - DateTime?

    //  Свойство - DeletedDate 
    // Назначение - Дата удаления комментария.
    // Тип данных - DateTime

    //  Свойство - PostId 
    // Назначение - ID записи, к которой комментарий относился. Может быть null.
    // Тип данных - int?

    public class DeletedCommentDto
    {
        public int CommentId { get; set; }
        public string Content { get; set; }
        public string AuthorName { get; set; } 
        public int? AuthorId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime DeletedDate { get; set; }
        public int? PostId { get; set; }
    }
}
