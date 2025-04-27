using Forum.Services.Interfaces;
using BCryptNet = BCrypt.Net.BCrypt;
//  Тип элемента - Класс Сервиса 
// Назначение типа - Реализует конкретный алгоритм или логику, определенную интерфейсом сервиса.
//  Назначение элемента - Сервис Хеширования Паролей с использованием BCrypt 
// Реализует интерфейс IPasswordHasher, используя библиотеку BCrypt.Net для создания безопасных хешей паролей и их проверки.

namespace Forum.Services
{
    //  Метод - HashPassword 
    // Назначение - Создает хеш пароля с использованием BCrypt.
    // Что делает - Вызывает статический метод BCryptNet.HashPassword, передавая ему пароль.
    // WorkFactor (workFactor - 11) определяет вычислительную сложность хеширования (баланс между безопасностью и производительностью).
    // Параметры - password - строка пароля для хеширования.
    // Возвращает - string - строка, содержащая BCrypt хеш пароля.

    //  Метод - VerifyPassword 
    // Назначение - Проверяет, соответствует ли предоставленный пароль BCrypt хешу.
    // Что делает - Вызывает статический метод BCryptNet.Verify, передавая ему введенный пароль и сохраненный хеш.
    // BCrypt выполняет сравнение.
    // Использует try-catch для обработки возможных исключений, если формат хеша некорректен.
    // Параметры -
    // hashedPassword - строка с BCrypt хешем из базы данных.
    // providedPassword - строка с паролем, введенным пользователем.
    // Возвращает - bool - true, если пароль совпадает с хешем, иначе false.

    public class BcryptPasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            return BCryptNet.HashPassword(password, workFactor: 11);
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            try
            {
                return BCryptNet.Verify(providedPassword, hashedPassword);
            }
            catch (BCrypt.Net.SaltParseException) 
            {
                return false;
            }
        }
    }
}
