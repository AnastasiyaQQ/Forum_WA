using System.ComponentModel.DataAnnotations;
//  Тип элемента - DTO (Data Transfer Object) 
// Назначение типа - DTO используются для передачи данных между слоями. 
// Классы, представляющие данные, получаемые от сервера и отправляемые на сервер.

//  Назначение элемента - Данные для входа пользователя 
// Используется для передачи имени пользователя и пароля к API при выполнении запроса на вход (/api/auth/login).

namespace Forum.Dtos.Auth
{
    //  Свойство - Username 
    // Назначение - Имя пользователя для входа.
    // Тип данных - string
    // Ограничения/Валидация - Обязательное поле ([Required]), максимальная длина 50 символов ([MaxLength(50)]).

    //  Свойство - Password 
    // Назначение - Пароль пользователя для входа.
    // Тип данных - string
    // Ограничения/Валидация - Обязательное поле ([Required]), минимальная длина 6 символов ([MinLength(6)]).

    public class LoginDto
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [MinLength(6)] 
        public string Password { get; set; }
    }
}
