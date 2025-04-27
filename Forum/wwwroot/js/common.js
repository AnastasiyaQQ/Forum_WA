//  Тип элемента - JavaScript Файл
// Назначение типа - Скрипт, выполняемый в браузере.

//  Назначение элемента - Общие функции
// Содержит вспомогательные функции, используемые на многих страницах фронтенда
//   Управление аутентификацией (токен, статус входа, выход) в localStorage.
//   Централизованная функция `fetchApi` для отправки запросов к бэкенд API и базовой обработки ответов/ошибок.
//   Функции для отображения/скрытия сообщений пользователю (`showMessage`, `hideMessage`).
//   Вспомогательные функции для работы с URL (`getQueryParam`), форматирования дат (`formatDate`).
//   Функции для динамического создания заголовка страницы (`setupHeader`), включая меню и поиск.
//   Обработчик поиска (`handleSearch`).
//   Функция отрисовки элементов пагинации (`renderPagination`).
//   Функция принудительной проверки входа (`enforceLogin`).
//   Настройка заголовка.

//  Ключевые Константы/Функции
// API_BASE_URL - Базовый URL бэкенд API.
// storeToken, getToken, getUserInfo, isLoggedIn, logout - Управление аутентификацией клиента.
// fetchApi - Основная функция для запросов к API (обрабатывает заголовки, токен, базовые ошибки).
// setupHeader - Создает HTML для заголовка (меню навигации, поиск, кнопка выхода).
// handleSearch - Обрабатывает ввод в поиске и нажатие кнопки, выполняет перенаправление на страницу результатов.
// renderPagination - Генерирует HTML для кнопок пагинации.

const API_BASE_URL = 'https://localhost:7184/api'; 
const UI_BASE_URL = 'https://localhost:7184'; 

// Аутентификация
function storeToken(tokenData) {
    localStorage.setItem('authToken', tokenData.accessToken);
    localStorage.setItem('authUser', JSON.stringify(tokenData.user));
    localStorage.setItem('tokenExpiry', tokenData.expiresAt);
}

function getToken() {

    return localStorage.getItem('authToken');
}

function getUserInfo() {
    const user = localStorage.getItem('authUser');
    return user ? JSON.parse(user) : null;
}

function isLoggedIn() {
    return !!getToken(); 
    // Проверка, существует ли токен 
}

function logout() {
    localStorage.removeItem('authToken');
    localStorage.removeItem('authUser');
    localStorage.removeItem('tokenExpiry');
    // Перенаправление на страницу входа
    window.location.href = '/login.html';
}

async function fetchApi(endpoint, method = 'GET', body = null, requiresAuth = true) {
    const headers = {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
    };

    const token = getToken();
    if (requiresAuth && token) {
        headers['Authorization'] = `Bearer ${token}`;
    } else if (requiresAuth && !token) {
        console.warn('Authentication required, redirecting to login.');
        logout(); 
        return Promise.reject({ status: 401, message: 'Unauthorized' }); 
    }


    const config = {
        method: method,
        headers: headers,
    };

    if (body && (method === 'POST' || method === 'PUT')) {
        config.body = JSON.stringify(body);
    }

    try {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, config);

        if (response.status === 401) {
            console.warn('API returned 401 Unauthorized, logging out.');
            logout(); 
            return Promise.reject({ status: 401, message: 'Unauthorized' });
        }
        if (response.status === 403) {
            console.warn('API returned 403 Forbidden.');
            alert("You don't have permission to perform this action.");
            return Promise.reject({ status: 403, message: 'Forbidden' });
        }

        if (response.status === 204) {
            return { success: true, status: 204 };
        }

        const data = await response.json();

        if (!response.ok) {
            const errorMessage = data?.message || data?.title || `Request failed with status ${response.status}`;
            console.error('API Error:', errorMessage, data);
            return Promise.reject({ status: response.status, message: errorMessage, data: data });
        }

        return data; 

    } catch (error) {
        console.error(`Fetch error for ${method} ${endpoint}:`, error);
        let errorMessage = 'An unexpected error occurred. Please check your network connection.';
        if (error instanceof TypeError && error.message === 'Failed to fetch') {
            errorMessage = 'Cannot connect to the server. Please ensure the API is running and accessible.';
        } else if (error.message) {
            errorMessage = error.message; 
        }
        alert(errorMessage);
        return Promise.reject({ status: 0, message: errorMessage }); 
    }
}

// --- UI Helpers ---
function showMessage(elementId, message, isError = false) {
    const element = document.getElementById(elementId);
    if (element) {
        element.textContent = message;
        element.className = isError ? 'error-message' : 'success-message';
        element.style.display = 'block';
    }
}

function hideMessage(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.style.display = 'none';
        element.textContent = '';
    }
}


function getQueryParam(param) {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get(param);
}

function formatDate(dateString) {
    if (!dateString) return 'N/A';
    try {
        const date = new Date(dateString);
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0'); 
        const day = String(date.getDate()).padStart(2, '0');
        return `${day}.${month}.${year}`; 
    } catch (e) {
        console.error("Error formatting date:", dateString, e);
        return 'Invalid Date';
    }
}


function setupHeader() {
    const headerPlaceholder = document.getElementById('header-placeholder');
    if (!headerPlaceholder) return;

    const userInfo = getUserInfo();
    const loggedIn = isLoggedIn();
    let menuHtml = '';
    let searchHtml = `
        <div class="search-bar">
            <input type="text" id="search-input" placeholder="Поиск...">
            <button id="search-button">🔍</button> 
        </div>`;

    if (loggedIn && userInfo) {
        // Меню входа в систему
        if (userInfo.role === 'Admin') {
            // Меню администратора
            menuHtml = `
                 <a href="/admin-main.html" class="${isActive('/admin-main.html')}">Главная</a>
                 <a href="/admin-posts.html" class="${isActive('/admin-posts.html')}">Записи</a>
                 <a href="/admin-comments.html" class="${isActive('/admin-comments.html')}">Комментарии</a>
                 <a href="/admin-deleted-posts.html" class="${isActive('/admin-deleted-posts.html')}">Удаленные записи</a>
                 <a href="/admin-deleted-comments.html" class="${isActive('/admin-deleted-comments.html')}">Удаленные комментарии</a>
                 <a href="#" id="logout-link">Выход (${userInfo.username})</a>
             `;
        } else {
            // Меню обычного пользователя
            menuHtml = `
                 <a href="/main.html" class="${isActive('/main.html')}">Главная</a>
                 <a href="/post-create.html" class="${isActive('/post-create.html')}">Новая запись</a>
                 <a href="/my-posts.html" class="${isActive('/my-posts.html')}">Мои записи</a>
                 <a href="/my-comments.html" class="${isActive('/my-comments.html')}">Мои комментарии</a>
                 <a href="#" id="logout-link">Выход (${userInfo.username})</a>
             `;
        }

    } else {
        menuHtml = '<a href="/login.html">Вход</a> <a href="/register.html">Регистрация</a>';
        searchHtml = '';
    }

    headerPlaceholder.innerHTML = `
        <div class="header">
             <div class="header-content">
                <nav class="nav-menu">
                    ${menuHtml}
                </nav>
                ${searchHtml}
            </div>
        </div>`;

    const logoutLink = document.getElementById('logout-link');
    if (logoutLink) {
        logoutLink.addEventListener('click', (e) => {
            e.preventDefault();
            logout();
        });
    }

    const searchButton = document.getElementById('search-button');
    const searchInput = document.getElementById('search-input');
    if (searchButton && searchInput) {
        searchButton.addEventListener('click', handleSearch);
        searchInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                handleSearch();
            }
        });
    }
    if (searchButton && searchInput) {
        searchButton.addEventListener('click', () => { 
            console.log("Search button clicked!"); 
            handleSearch();
        });
        searchInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                console.log("Enter pressed in search input!"); 
                handleSearch();
            }
        });
    }
}

function handleSearch() {
    const searchInput = document.getElementById('search-input');
    const searchTerm = searchInput.value.trim();
    /// Определить текущую страницу
    const currentPage = window.location.pathname;
    let targetPage = '/main.html'; 

    if (currentPage.startsWith('/admin')) {
        targetPage = '/admin-main.html'; 
    }

    window.location.href = `${targetPage}?search=${encodeURIComponent(searchTerm)}`;
}

function isActive(path) {
    return window.location.pathname === path ? 'active' : '';
}


function renderPagination(containerId, pagedResult, loadFunction) {
    const paginationContainer = document.getElementById(containerId);
    if (!paginationContainer || !pagedResult || pagedResult.totalPages <= 1) {
        if (paginationContainer) paginationContainer.innerHTML = ''; 
        return;
    }

    const currentPage = pagedResult.pageNumber;
    const totalPages = pagedResult.totalPages;

    let paginationHtml = '';

    paginationHtml += `<button onclick="${loadFunction.name}(${currentPage - 1})" ${currentPage === 1 ? 'disabled' : ''}><<</button>`;

    paginationHtml += `<span>Страница ${currentPage} из ${totalPages}</span>`;

    paginationHtml += `<button onclick="${loadFunction.name}(${currentPage + 1})" ${currentPage === totalPages ? 'disabled' : ''}>>></button>`;

    paginationContainer.innerHTML = paginationHtml;
}

function enforceLogin() {
    const isAuthPage = window.location.pathname.includes('login.html') || window.location.pathname.includes('register.html');
    if (!isLoggedIn() && !isAuthPage) {
        window.location.href = '/login.html';
    }
}

document.addEventListener('DOMContentLoaded', () => {
    setupHeader();
});
