﻿using System.ComponentModel.DataAnnotations;
//  Тип элемента - DTO (Data Transfer Object) 
// Назначение типа - DTO используются для передачи данных между слоями. 
// Классы, представляющие данные, получаемые от сервера и отправляемые на сервер.

//  Назначение элемента - Данные для создания нового комментария 
// Используется для передачи текста нового комментария к API при вызове создания комментария.

namespace Forum.Dtos.Comments
{
    //  Свойство - Content 
    // Назначение - Содержание (текст) для создаваемого комментария.
    // Тип данных - string
    // Ограничения/Валидация - Обязательное поле ([Required]), максимальная длина 2000 символов ([MaxLength(2000)]).

    public class CreateCommentDto
    {
        [Required]
        [MaxLength(2000)]
        public string Content { get; set; }
    }
}
