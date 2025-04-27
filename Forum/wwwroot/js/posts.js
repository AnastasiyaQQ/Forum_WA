//  Тип элемента - JavaScript Файл
// Назначение типа - Скрипт, выполняемый в браузере.

//  Назначение элемента - Логика для страниц, связанных с записями (постами)
// Содержит JavaScript код для страниц:
//   Главная (main.html) - загрузка и отображение списка постов.
//   Просмотр поста (post-view.html) - загрузка и отображение деталей одного поста.
//   Создание поста (post-create.html) - обработка формы создания.
//   Мои записи (my-posts.html) - загрузка и отображение постов текущего пользователя, управление (редактирование, удаление).
//   Редактирование поста (post-edit.html) - загрузка данных поста в форму и обработка обновления.
// Использует функции из common.js.

//  Ключевые Функции
// loadPosts(page) - Загружает посты для главной страницы (GET /api/posts).
// loadMyPosts(page) - Загружает посты текущего пользователя (GET /api/posts/my).
// renderPostList(container, posts, isMyPostsView) - Генерирует HTML для списка постов (с кнопками управления, если isMyPostsView = true).
// loadPostDetails() - Загружает детали одного поста (GET /api/posts/{id}).
// setupCreateForm() - Добавляет обработчик отправки для формы создания поста (POST /api/posts).
// loadPostForEdit() - Загружает данные поста для формы редактирования (GET /api/posts/{id}).
// setupEditForm() - Добавляет обработчик отправки для формы редактирования поста (PUT /api/posts/{id}).
// confirmDeletePost(postId, isMyPostsView) - Запрашивает подтверждение и удаляет пост (DELETE /api/posts/{id}), затем перезагружает нужный список.

//  Логика при загрузке
//   Вызывает enforceLogin() для проверки аутентификации (кроме публичных страниц).
//   Определяет текущую страницу по URL (window.location.pathname).
//   Вызывает соответствующую функцию для загрузки данных или настройки формы для текущей страницы.

//  Зависимости
//  common.js
//  HTML страницы - main.html, post-view.html, post-create.html, my-posts.html, post-edit.html.


document.addEventListener('DOMContentLoaded', () => {
    enforceLogin(); // Проверка - пользователь вошел в систему для просмотра страниц, связанных с постами 

    const path = window.location.pathname;

    if (path.includes('main.html') || path === '/') {
        loadPosts(); // Загрузка посты для главной страницы
    } else if (path.includes('post-view.html')) {
        loadPostDetails();
    } else if (path.includes('post-create.html')) {
        setupCreateForm();
    } else if (path.includes('my-posts.html')) {
        loadMyPosts();
    } else if (path.includes('post-edit.html')) {
        loadPostForEdit();
        setupEditForm();
    }
});

// Определения функций

async function loadPosts(page = 1) {
    const postList = document.getElementById('post-list');
    const paginationContainer = document.getElementById('pagination');
    const searchTerm = getQueryParam('search') || ''; 
    if (postList) postList.innerHTML = 'Loading posts...';
    if (paginationContainer) paginationContainer.innerHTML = '';


    // Обновить поле ввода поиска, если поисковый запрос существует
    const searchInput = document.getElementById('search-input');
    if (searchInput && searchTerm) {
        searchInput.value = searchTerm;
    }


    try {
        const result = await fetchApi(`/posts?pageNumber=${page}&pageSize=5&searchTerm=${encodeURIComponent(searchTerm)}`, 'GET', null, false); // No auth needed to view
        renderPostList(postList, result.items);
        renderPagination('pagination', result, loadPosts); 
    } catch (error) {
        if (postList) postList.innerHTML = `<p class="error-message">Failed to load posts: ${error.message}</p>`;
    }
}

async function loadMyPosts(page = 1) {
    const postList = document.getElementById('post-list');
    const paginationContainer = document.getElementById('pagination');
    const searchTerm = getQueryParam('search') || '';
    if (postList) postList.innerHTML = 'Loading your posts...';
    if (paginationContainer) paginationContainer.innerHTML = '';

    // Обновить поле ввода поиска, если поисковый запрос существует
    const searchInput = document.getElementById('search-input');
    if (searchInput && searchTerm) {
        searchInput.value = searchTerm;
    }


    try {
        const result = await fetchApi(`/posts/my?pageNumber=${page}&pageSize=5&searchTerm=${encodeURIComponent(searchTerm)}`, 'GET', null, true);
        renderPostList(postList, result.items, true); 
        renderPagination('pagination', result, loadMyPosts);
    } catch (error) {
        if (error.status !== 401 && error.status !== 403) { 
            if (postList) postList.innerHTML = `<p class="error-message">Failed to load your posts: ${error.message}</p>`;
        }
    }
}


function renderPostList(container, posts, isMyPostsView = false) {
    if (!container) return;
    if (posts.length === 0) {
        container.innerHTML = '<p>No posts found.</p>';
        return;
    }

    let postsHtml = '';
    posts.forEach(post => {
        postsHtml += `
            <div class="post-item">
                <div class="post-item-header">
                    <h3><a href="/post-view.html?id=${post.id}">${post.title || 'Untitled'}</a></h3>
                    <div class="item-meta">
                        <span>Автор: ${post.authorName || 'Unknown'}</span>
                        <span>Дата создания: ${formatDate(post.createdDate)}</span>
                    </div>
                </div>
                ${isMyPostsView ? `
                <div class="item-actions">
                    <button onclick="location.href='/post-edit.html?id=${post.id}'">Изменить данные</button>
                    <button onclick="confirmDeletePost(${post.id}, true)">Удалить запись</button>
                </div>
                ` : ''}
            </div>
        `;
    });
    container.innerHTML = postsHtml;
}

async function loadPostDetails() {
    const postId = getQueryParam('id');
    const container = document.getElementById('post-details-container');
    if (!postId || !container) {
        if (container) container.innerHTML = '<p class="error-message">Post ID not provided or container not found.</p>';
        return;
    }
    container.innerHTML = 'Loading post...';

    try {
        const post = await fetchApi(`/posts/${postId}`, 'GET', null, false); 

        // Отобразить детали поста
        container.innerHTML = `
            <h2>${post.title}</h2>
            <p class="post-content-full">${post.content.replace(/\n/g, '<br>')}</p> <!-- Display line breaks -->
            <div class="post-footer">
                 <div>
                    <span class="comment-icon" onclick="location.href='/post-comments.html?postId=${post.id}'" title="View Comments">💬</span>
                    Автор: ${post.authorName}
                 </div>
                <span>Дата создания: ${formatDate(post.createdDate)}</span>
            </div>
        `;
    } catch (error) {
        container.innerHTML = `<p class="error-message">Failed to load post details: ${error.message}</p>`;
    }
}


function setupCreateForm() {
    const form = document.getElementById('create-post-form');
    if (!form) return;

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        hideMessage('form-message');
        const title = document.getElementById('title').value;
        const content = document.getElementById('content').value;
        const button = form.querySelector('button[type="submit"]');
        button.disabled = true;
        button.textContent = 'Сохранение...';


        try {
            const newPost = await fetchApi('/posts', 'POST', { title, content });
            showMessage('form-message', 'Запись успешно создана!', false);
            setTimeout(() => { window.location.href = `/post-view.html?id=${newPost.id}`; }, 1500);
        } catch (error) {
            showMessage('form-message', `Ошибка сохранения: ${error.message}`, true);
            button.disabled = false;
            button.textContent = 'Сохранить';
        }
    });
}


async function loadPostForEdit() {
    const postId = getQueryParam('id');
    const titleInput = document.getElementById('title');
    const contentInput = document.getElementById('content');
    const formMessage = document.getElementById('form-message');


    if (!postId || !titleInput || !contentInput) {
        if (formMessage) showMessage('form-message', 'Error: Could not find post ID or form fields.', true);
        return;
    }

    try {
        // Получить данные о посте
        const post = await fetchApi(`/posts/${postId}`, 'GET', null, true);

        const userInfo = getUserInfo();
        if (userInfo && post.authorId !== userInfo.id && userInfo.role !== 'Admin') {
            showMessage('form-message', 'You do not have permission to edit this post.', true);
            titleInput.disabled = true;
            contentInput.disabled = true;
            document.querySelector('#edit-post-form button[type="submit"]').disabled = true;
            return;
        }

        titleInput.value = post.title;
        contentInput.value = post.content;
    } catch (error) {
        showMessage('form-message', `Failed to load post for editing: ${error.message}`, true);
    }
}

function setupEditForm() {
    const form = document.getElementById('edit-post-form');
    const postId = getQueryParam('id');
    if (!form || !postId) return;


    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        hideMessage('form-message');
        const title = document.getElementById('title').value;
        const content = document.getElementById('content').value;
        const button = form.querySelector('button[type="submit"]');
        button.disabled = true;
        button.textContent = 'Сохранение...';

        try {
            await fetchApi(`/posts/${postId}`, 'PUT', { title, content });
            showMessage('form-message', 'Запись успешно обновлена!', false);
            setTimeout(() => { window.location.href = '/my-posts.html'; }, 1500);
        } catch (error) {
            showMessage('form-message', `Ошибка обновления: ${error.message}`, true);
            button.disabled = false;
            button.textContent = 'Сохранить';
        }
    });
}


async function confirmDeletePost(postId, isMyPostsView = false) {
    if (confirm('Вы уверены, что хотите удалить эту запись? Это действие нельзя отменить.')) {
        try {
            const result = await fetchApi(`/posts/${postId}`, 'DELETE');
            alert(result.message || 'Запись успешно удалена.'); 
            if (isMyPostsView) {
                loadMyPosts();
            } else if (window.location.pathname.includes('/admin-posts.html')) {
                alert("Admin delete function called - reload needed."); 
                window.location.reload(); 
            } else {
                loadPosts();
            }
        } catch (error) {
            alert(`Ошибка удаления: ${error.message}`);
        }
    }
}
