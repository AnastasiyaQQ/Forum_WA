//  Тип элемента - JavaScript Файл
// Назначение типа - Скрипт, выполняемый в браузере.

//  Назначение элемента - Логика для страниц, связанных с комментариями
// Содержит JavaScript код для страниц:
// Просмотр комментариев к посту (post-comments.html) - загрузка и отображение списка комментариев к конкретному посту.
// Создание комментария (comment-create.html) - обработка формы создания.
// Мои комментарии (my-comments.html) - загрузка и отображение комментариев текущего пользователя, управление ими.
// Редактирование комментария (comment-edit.html) - обработка формы редактирования.
// Использует функции из common.js.

//  Ключевые Функции
// loadCommentsForPost(page) - Загружает комментарии для конкретного поста (GET /api/posts/{postId}/comments).
// loadMyComments(page) - Загружает комментарии текущего пользователя (GET /api/comments/my).
// renderCommentList(container, comments, isMyCommentsView) - Генерирует HTML для списка комментариев (с кнопками управления, если isMyCommentsView = true).
// setupCommentCreateForm() - Добавляет обработчик отправки для формы создания комментария (POST /api/posts/{postId}/comments).
// setupCommentEditForm() - Добавляет обработчик отправки для формы редактирования комментария (PUT /api/comments/{id}).
// confirmDeleteComment(commentId, isMyCommentsView) - Запрашивает подтверждение и удаляет комментарий (DELETE /api/comments/{id}), затем перезагружает нужный список.

//  Логика при загрузке 
//   Вызывает enforceLogin() для проверки аутентификации.
//   Определяет текущую страницу по URL.
//   Вызывает соответствующую функцию для загрузки данных или настройки формы.

//  Зависимости
//  common.js
//  HTML страницы - post-comments.html, comment-create.html, my-comments.html, comment-edit.html.

document.addEventListener('DOMContentLoaded', () => {
    enforceLogin();

    const path = window.location.pathname;

    if (path.includes('post-comments.html')) {
    } else if (path.includes('comment-create.html')) {
        setupCommentCreateForm();
    } else if (path.includes('my-comments.html')) {
        loadMyComments();
    } else if (path.includes('comment-edit.html')) {
        setupCommentEditForm(); 
    }
});

// Определения функций

async function loadCommentsForPost(page = 1) {
    const postId = getQueryParam('postId');
    const commentList = document.getElementById('comment-list');
    const paginationContainer = document.getElementById('pagination');
    const postTitlePlaceholder = document.getElementById('post-title-placeholder'); 
    // ​​Обработка поискового запроса
    const searchTerm = getQueryParam('search') || ''; 

    if (!postId) {
        if (commentList) commentList.innerHTML = '<p class="error-message">Post ID not found in URL.</p>';
        return;
    }

    if (commentList) commentList.innerHTML = 'Loading comments...';
    if (paginationContainer) paginationContainer.innerHTML = '';

    if (postTitlePlaceholder) {
        fetchApi(`/posts/${postId}`, 'GET', null, false) 
            .then(post => { postTitlePlaceholder.textContent = `Комментарии к записи: ${post.title}`; })
            .catch(err => { console.warn("Could not load post title:", err.message); });
    }
    // Обновить поле ввода поиска, если поисковый запрос существует
    const searchInput = document.getElementById('search-input');
    if (searchInput && searchTerm) {
        searchInput.value = searchTerm;
    }


    try {
        const result = await fetchApi(`/posts/${postId}/comments?pageNumber=${page}&pageSize=10&searchTerm=${encodeURIComponent(searchTerm)}`, 'GET', null, false);
        renderCommentList(commentList, result.items); 
        renderPagination('pagination', result, loadCommentsForPost);
    } catch (error) {
        if (commentList) commentList.innerHTML = `<p class="error-message">Failed to load comments: ${error.message}</p>`;
    }
}

async function loadMyComments(page = 1) {
    const commentList = document.getElementById('comment-list');
    const paginationContainer = document.getElementById('pagination');
    const searchTerm = getQueryParam('search') || '';
    if (commentList) commentList.innerHTML = 'Loading your comments...';
    if (paginationContainer) paginationContainer.innerHTML = '';

    // Обновить поле ввода поиска, если поисковый запрос существует
    const searchInput = document.getElementById('search-input');
    if (searchInput && searchTerm) {
        searchInput.value = searchTerm;
    }


    try {
        
        const result = await fetchApi(`/comments/my?pageNumber=${page}&pageSize=10&searchTerm=${encodeURIComponent(searchTerm)}`, 'GET', null, true);
        renderCommentList(commentList, result.items, true); 
        // true для кнопок редактирования/удаления
        renderPagination('pagination', result, loadMyComments);
    } catch (error) {
        if (error.status !== 401 && error.status !== 403) {
            if (commentList) commentList.innerHTML = `<p class="error-message">Failed to load your comments: ${error.message}</p>`;
        }
    }
}


function renderCommentList(container, comments, isMyCommentsView = false) {
    if (!container) return;
    if (!comments || comments.length === 0) {
        container.innerHTML = '<p>Комментарии не найдены.</p>';
        return;
    }

    let commentsHtml = '';
    const userInfo = getUserInfo(); 

    comments.forEach(comment => {
        let showActions = isMyCommentsView;

        commentsHtml += `
            <div class="comment-item" id="comment-${comment.id}">
                <div class="comment-item-content">
                    ${comment.content.replace(/\n/g, '<br>')}
                </div>
                <div class="comment-item-header">
                    <span></span> <!-- Placeholder for alignment -->
                    <div class="item-meta">
                        <span>Автор: ${comment.authorName || 'Unknown'}</span>
                        <span>Дата создания: ${formatDate(comment.createdDate)}</span>
                    </div>
                </div>
                 ${showActions ? `
                 <div class="item-actions">
                     <button onclick="location.href='/comment-edit.html?id=${comment.id}'">Изменить данные</button>
                     <button onclick="confirmDeleteComment(${comment.id}, ${isMyCommentsView})">Удалить комментарий</button>
                 </div>
                 ` : ''}
            </div>
        `;
    });
    container.innerHTML = commentsHtml;
}


function setupCommentCreateForm() {
    const form = document.getElementById('create-comment-form');
    const postId = getQueryParam('postId');
    if (!form || !postId) {
        if (form) showMessage('form-message', 'Ошибка: Не удалось найти форму или ID записи.', true);
        return;
    }

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        hideMessage('form-message');
        const content = document.getElementById('content').value;
        const button = form.querySelector('button[type="submit"]');
        button.disabled = true;
        button.textContent = 'Сохранение...';

        if (!content.trim()) {
            showMessage('form-message', 'Комментарий не может быть пустым.', true);
            button.disabled = false;
            button.textContent = 'Сохранить';
            return;
        }

        try {
            // Требуется аутентификация
            const newComment = await fetchApi(`/posts/${postId}/comments`, 'POST', { content });
            showMessage('form-message', 'Комментарий успешно добавлен!', false);
            // Перенаправление обратно на страницу комментариев 
            setTimeout(() => { window.location.href = `/post-comments.html?postId=${postId}`; }, 1500);
        } catch (error) {
            showMessage('form-message', `Ошибка сохранения: ${error.message}`, true);
            button.disabled = false;
            button.textContent = 'Сохранить';
        }
    });
}

function setupCommentEditForm() {
    const form = document.getElementById('edit-comment-form');
    const commentId = getQueryParam('id');

    if (!form || !commentId) {
        if (form) showMessage('form-message', 'Ошибка: Не удалось найти форму или ID комментария.', true);
        return;
    }


    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        hideMessage('form-message');
        const content = document.getElementById('content').value;
        const button = form.querySelector('button[type="submit"]');
        button.disabled = true;
        button.textContent = 'Сохранение...';

        if (!content.trim()) {
            showMessage('form-message', 'Комментарий не может быть пустым.', true);
            button.disabled = false;
            button.textContent = 'Сохранить';
            return;
        }

        try {
            // Требуется аутентификация, API проверяет владельца/роль
            await fetchApi(`/comments/${commentId}`, 'PUT', { content });
            showMessage('form-message', 'Комментарий успешно обновлен!', false);
            // Перенаправление обратно в «Мои комментарии» после
            setTimeout(() => { window.location.href = '/my-comments.html'; }, 1500);
        } catch (error) {
            showMessage('form-message', `Ошибка обновления: ${error.message}`, true);
            button.disabled = false;
            button.textContent = 'Сохранить';
        }
    });
}


async function confirmDeleteComment(commentId, isMyCommentsView = false) {
    if (confirm('Вы уверены, что хотите удалить этот комментарий? Это действие нельзя отменить.')) {
        try {
            // Требуется аутентификация, API проверяет владельца/роль
            const result = await fetchApi(`/comments/${commentId}`, 'DELETE');
            alert(result.message || 'Комментарий успешно удален.');

            
            if (isMyCommentsView) {
                loadMyComments(); 
            } else if (window.location.pathname.includes('/admin-comments.html')) {
                if (typeof loadAdminComments === "function") loadAdminComments();
                else window.location.reload();
            } else if (window.location.pathname.includes('/post-comments.html')) {
                loadCommentsForPost(); // Обновить комментарии к текущему посту
            } else {
                console.warn("Deleted comment, but unsure which list to reload.");
                window.location.reload(); 
            }

        } catch (error) {
            alert(`Ошибка удаления: ${error.message}`);
        }
    }
}
