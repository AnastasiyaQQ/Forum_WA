using System.ComponentModel.DataAnnotations;
//  Тип элемента - DTO (Data Transfer Object) 
// Назначение типа - DTO используются для передачи данных между слоями. 
// Классы, представляющие данные, получаемые от сервера и отправляемые на сервер.

//  Назначение элемента - Данные для регистрации пользователя 
// Используется для передачи желаемого имени пользователя и пароля к API при выполнении запроса на регистрацию (/api/auth/register).

namespace Forum.Dtos.Auth
{
    //  Свойство - Username 
    // Назначение - Желаемое имя пользователя для нового аккаунта.
    // Тип данных - string
    // Ограничения/Валидация - Обязательное поле ([Required]), максимальная длина 50 символов ([MaxLength(50)]).

    //  Свойство - Password 
    // Назначение - Желаемый пароль для нового аккаунта.
    // Тип данных - string
    // Ограничения/Валидация - Обязательное поле ([Required]), минимальная длина 6 символов ([MinLength(6)]).

    public class RegisterDto
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }
}
