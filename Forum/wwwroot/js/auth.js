//  Тип элемента - JavaScript Файл
// Назначение типа - Скрипт, выполняемый в браузере.

//  Назначение элемента - Логика для страниц аутентификации
// Содержит JavaScript код для страниц входа (login.html) и регистрации (register.html).
// Использует функции из common.js (isLoggedIn, fetchApi, storeToken, showMessage и др.).

//  Основная логика
// 1. При загрузке
//   Проверяет, если пользователь уже вошел в систему, и перенаправляет его на соответствующую главную страницу (пользователя или админа).
//   Находит формы входа и регистрации по ID.
//   Добавляет обработчики событий 'submit' к этим формам.
// 2. Обработчик формы входа -
//   Получает имя пользователя и пароль из полей ввода.
//   Вызывает `fetchApi` для отправки запроса POST /api/auth/login.
//   При успехе - вызывает `storeToken` для сохранения токена и `getUserInfo`, затем перенаправляет на главную страницу (пользователя/админа).
//   При ошибке - отображает сообщение об ошибке с помощью `showMessage`.
// 3. Обработчик формы регистрации -
//   Получает имя пользователя и пароль.
//   Вызывает `fetchApi` для отправки запроса POST /api/auth/register.
//   При успехе - отображает сообщение об успехе и через короткую задержку перенаправляет на страницу входа.
//   При ошибке - отображает сообщение об ошибке.

//  Зависимости
// common.js (для fetchApi, storeToken, isLoggedIn, getUserInfo, showMessage, hideMessage и т.д.)
// HTML страницы login.html и register.html (ожидает наличие форм с ID 'login-form', 'register-form' и полей с ID 'username', 'password', а также элементов для сообщений с ID 'login-error', 'register-message').

document.addEventListener('DOMContentLoaded', () => {
    if (isLoggedIn() && (window.location.pathname.includes('login.html') || window.location.pathname.includes('register.html'))) {
        const user = getUserInfo();
        window.location.href = user?.role === 'Admin' ? '/admin-main.html' : '/main.html';
        return; 
    }

    // Логика страницы входа
    const loginForm = document.getElementById('login-form');
    if (loginForm) {
        loginForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            hideMessage('login-error');
            const username = document.getElementById('username').value;
            const password = document.getElementById('password').value;

            try {
                const tokenData = await fetchApi('/auth/login', 'POST', { username, password }, false); // Вход не требует аутентификации
                storeToken(tokenData);
                // Перенаправление на основе роли
                const user = getUserInfo();
                window.location.href = user?.role === 'Admin' ? '/admin-main.html' : '/main.html';
            } catch (error) {
                showMessage('login-error', error.message || 'Login failed. Please check username and password.', true);
            }
        });
    }

    // Логика страницы регистрации
    const registerForm = document.getElementById('register-form');
    if (registerForm) {
        registerForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            hideMessage('register-message');
            const username = document.getElementById('username').value;
            const password = document.getElementById('password').value;

            try {
                const result = await fetchApi('/auth/register', 'POST', { username, password }, false);
                showMessage('register-message', result.message || 'Registration successful! Redirecting to login...', false);
                // Перенаправление на страницу входа
                setTimeout(() => { window.location.href = '/login.html'; }, 2000);
            } catch (error) {
                showMessage('register-message', error.message || 'Registration failed.', true);
            }
        });
    }
});
