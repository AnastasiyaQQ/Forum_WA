//  Тип элемента - JavaScript Файл
// Назначение типа - Скрипт, выполняемый в браузере.

//  Назначение элемента - Логика для администраторских страниц
// Содержит JavaScript код для страниц, доступных только администраторам:
//  Главная админская (admin-main.html) - отображение всех постов.
//  Управление записями (admin-posts.html) - отображение всех постов с возможностью удаления.
//  Управление комментариями (admin-comments.html) - отображение всех комментариев с возможностью удаления.
//  Удаленные записи (admin-deleted-posts.html) - просмотр списка удаленных записей.
//  Просмотр удаленной записи (admin-deleted-post-view.html) - просмотр деталей удаленной записи.
//  Удаленные комментарии (admin-deleted-comments.html) - просмотр списка удаленных комментариев.
// Использует функции из common.js и, возможно, функции подтверждения удаления из posts.js/comments.js.

//  Ключевые Функции
// loadAdminPosts(page) - Загружает все активные посты (GET /api/posts или /api/admin/posts).
// renderAdminPostList(container, posts) - Генерирует HTML для списка постов с кнопкой удаления.
// confirmAdminDeletePost(postId) - Обрабатывает удаление поста администратором (DELETE /api/posts/{id}).
// loadAdminComments(page) - Загружает ВСЕ активные комментарии (GET /api/admin/comments).
// renderAdminCommentList(container, comments) - Генерирует HTML для списка комментариев с кнопкой удаления и ссылкой на пост.
// confirmAdminDeleteComment(commentId) - Обрабатывает удаление комментария администратором (DELETE /api/comments/{id}).
// loadDeletedPosts(page) - Загружает список удаленных постов (GET /api/admin/deleted/posts).
// renderDeletedPostList(container, posts) - Генерирует HTML для списка удаленных постов.
// loadDeletedPostDetails() - Загружает детали удаленного поста (GET /api/admin/deleted/posts/{postId}).
// loadDeletedComments(page) - Загружает список удаленных комментариев (GET /api/admin/deleted/comments).
// renderDeletedCommentList(container, comments) - Генерирует HTML для списка удаленных комментариев.

//  Логика при загрузке DOM
//   Вызывает enforceLogin() и дополнительно проверяет роль "Admin". Если не админ - принудительный выход.
//   Определяет текущую админскую страницу по URL.
//   Вызывает соответствующую функцию загрузки данных для текущей страницы.

//  Зависимости
//   common.js
//   HTML страницы админского раздела.

document.addEventListener('DOMContentLoaded', () => {
    enforceLogin();
    const userInfo = getUserInfo();
    if (!userInfo || userInfo.role !== 'Admin') {
        alert('Access Denied: Admins only.');
        // Перенаправление на страницу входа
        logout(); 
        return;
    }

    const path = window.location.pathname;

    if (path.includes('admin-main.html')) {
        loadAdminPosts(); 
    } else if (path.includes('admin-posts.html')) {
        loadAdminPosts(); 
    } else if (path.includes('admin-comments.html')) {
        loadAdminComments(); 
    } else if (path.includes('admin-deleted-posts.html')) {
        loadDeletedPosts();
    } else if (path.includes('admin-deleted-post-view.html')) {
        loadDeletedPostDetails();
    } else if (path.includes('admin-deleted-comments.html')) {
        loadDeletedComments();
    }
});

// Определения функций

async function loadAdminPosts(page = 1) {
    const postList = document.getElementById('admin-post-list');
    const paginationContainer = document.getElementById('pagination');
    const searchTerm = getQueryParam('search') || '';
    if (postList) postList.innerHTML = 'Loading posts...';
    if (paginationContainer) paginationContainer.innerHTML = '';

    // Обновить поле ввода поиска, если поисковый запрос существуетs
    const searchInput = document.getElementById('search-input');
    if (searchInput && searchTerm) {
        searchInput.value = searchTerm;
    }


    try {
        
        const result = await fetchApi(`/posts?pageNumber=${page}&pageSize=5&searchTerm=${encodeURIComponent(searchTerm)}`, 'GET', null, true); // Auth required for admin view consistency
        renderAdminPostList(postList, result.items);
        renderPagination('pagination', result, loadAdminPosts);
    } catch (error) {
        if (error.status !== 401 && error.status !== 403) {
            if (postList) postList.innerHTML = `<p class="error-message">Failed to load posts: ${error.message}</p>`;
        }
    }
}

function renderAdminPostList(container, posts) {
    if (!container) return;
    if (!posts || posts.length === 0) {
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
                <div class="item-actions">
                    <button onclick="confirmAdminDeletePost(${post.id})">Удалить запись</button>
                </div>
            </div>
        `;
    });
    container.innerHTML = postsHtml;
}

async function confirmAdminDeletePost(postId) {
    if (confirm('Вы уверены, что хотите удалить эту запись (Admin)? Это действие нельзя отменить.')) {
        try {
            // API проверяет роль администратора
            const result = await fetchApi(`/posts/${postId}`, 'DELETE'); 
            // API handles admin permission
            alert(result.message || 'Запись успешно удалена (Admin).');
            loadAdminPosts(); 
        } catch (error) {
            alert(`Ошибка удаления: ${error.message}`);
        }
    }
}


async function loadAdminComments(page = 1) {
    const commentList = document.getElementById('admin-comment-list');
    const paginationContainer = document.getElementById('pagination');
    const searchTerm = getQueryParam('search') || '';
    if (commentList) commentList.innerHTML = 'Loading comments...';
    if (paginationContainer) paginationContainer.innerHTML = '';

    // Обновить поле ввода поиска, если поисковый запрос существует
    const searchInput = document.getElementById('search-input');
    if (searchInput && searchTerm) {
        searchInput.value = searchTerm;
    }


    try {
        const result = await fetchApi(`/admin/comments?pageNumber=${page}&pageSize=10&searchTerm=${encodeURIComponent(searchTerm)}`, 'GET', null, true); // Requires Admin Auth
        renderAdminCommentList(commentList, result.items);
        renderPagination('pagination', result, loadAdminComments);
    } catch (error) {
        if (error.status !== 401 && error.status !== 403) {
            if (commentList) commentList.innerHTML = `<p class="error-message">Failed to load comments: ${error.message}. Ensure API endpoint /api/admin/comments exists.</p>`;
        }
    }
}

function renderAdminCommentList(container, comments) {
    if (!container) return;
    if (!comments || comments.length === 0) {
        container.innerHTML = '<p>No comments found.</p>';
        return;
    }

    let commentsHtml = '';
    comments.forEach(comment => {
        commentsHtml += `
            <div class="comment-item" id="comment-${comment.id}">
                <div class="comment-item-content">
                     <small>(<a href="/post-view.html?id=${comment.postId}" title="Перейти к записи">Запись #${comment.postId}</a>)</small><br>
                    ${comment.content.replace(/\n/g, '<br>')}
                </div>
                 <div class="comment-item-header">
                    <span></span>
                    <div class="item-meta">
                        <span>Автор: ${comment.authorName || 'Unknown'}</span>
                        <span>Дата создания: ${formatDate(comment.createdDate)}</span>
                    </div>
                </div>
                <div class="item-actions">
                    <button onclick="confirmAdminDeleteComment(${comment.id})">Удалить комментарий</button>
                </div>
            </div>
        `;
    });
    container.innerHTML = commentsHtml;
}

async function confirmAdminDeleteComment(commentId) {
    if (confirm('Вы уверены, что хотите удалить этот комментарий (Admin)? Это действие нельзя отменить.')) {
        try {
            const result = await fetchApi(`/comments/${commentId}`, 'DELETE');
            // API обрабатывает разрешения администратора
            alert(result.message || 'Комментарий успешно удален (Admin).');
            loadAdminComments(); 
        } catch (error) {
            alert(`Ошибка удаления: ${error.message}`);
        }
    }
}


// Удаленные элементы

async function loadDeletedPosts(page = 1) {
    const listContainer = document.getElementById('deleted-post-list');
    const paginationContainer = document.getElementById('pagination');
    const searchTerm = getQueryParam('search') || '';
    if (listContainer) listContainer.innerHTML = 'Loading deleted posts...';
    if (paginationContainer) paginationContainer.innerHTML = '';

    // Обновить поле ввода поиска, если поисковый запрос существует
    const searchInput = document.getElementById('search-input');
    if (searchInput && searchTerm) {
        searchInput.value = searchTerm;
    }


    try {
        const result = await fetchApi(`/admin/deleted/posts?pageNumber=${page}&pageSize=5&searchTerm=${encodeURIComponent(searchTerm)}`, 'GET', null, true);
        renderDeletedPostList(listContainer, result.items);
        renderPagination('pagination', result, loadDeletedPosts);
    } catch (error) {
        if (error.status !== 401 && error.status !== 403) {
            if (listContainer) listContainer.innerHTML = `<p class="error-message">Failed to load deleted posts: ${error.message}</p>`;
        }
    }
}

function renderDeletedPostList(container, posts) {
    if (!container) return;
    if (!posts || posts.length === 0) {
        container.innerHTML = '<p>Удаленные записи не найдены.</p>';
        return;
    }
    let postsHtml = '';
    posts.forEach(post => {
        postsHtml += `
             <div class="deleted-item">
                 <div class="deleted-item-header">
                     <h3><a href="/admin-deleted-post-view.html?id=${post.postId}">${post.title || 'Untitled'}</a></h3>
                     <div class="item-meta">
                         <span>Автор: ${post.authorName || 'User Deleted/Unknown'}</span>
                         <span>Дата удаления: ${formatDate(post.deletedDate)}</span>
                     </div>
                 </div>
             </div>
         `;
    });
    container.innerHTML = postsHtml;
}


async function loadDeletedPostDetails() {
    const postId = getQueryParam('id');
    const container = document.getElementById('deleted-post-details');
    if (!postId || !container) {
        if (container) container.innerHTML = '<p class="error-message">Post ID not provided or container not found.</p>';
        return;
    }
    container.innerHTML = 'Loading deleted post details...';

    try {
        const post = await fetchApi(`/admin/deleted/posts/${postId}`, 'GET', null, true);

        container.innerHTML = `
            <h2>${post.title}</h2>
            <p class="post-content-full">${post.content ? post.content.replace(/\n/g, '<br>') : '<i>No content available</i>'}</p>
            <div class="post-footer">
                 <div>
                     Автор: ${post.authorName || 'User Deleted/Unknown'} (ID: ${post.authorId || 'N/A'})
                 </div>
                 <div>
                     <span>Дата создания: ${formatDate(post.createdDate)}</span>
                     <span>Дата удаления: ${formatDate(post.deletedDate)}</span>
                 </div>
            </div>
        `;
    } catch (error) {
        if (error.status !== 401 && error.status !== 403) {
            container.innerHTML = `<p class="error-message">Failed to load deleted post details: ${error.message}</p>`;
        }
    }
}


async function loadDeletedComments(page = 1) {
    const listContainer = document.getElementById('deleted-comment-list');
    const paginationContainer = document.getElementById('pagination');
    const searchTerm = getQueryParam('search') || '';
    if (listContainer) listContainer.innerHTML = 'Loading deleted comments...';
    if (paginationContainer) paginationContainer.innerHTML = '';

    // Обновить поле ввода поиска, если поисковый запрос существует
    const searchInput = document.getElementById('search-input');
    if (searchInput && searchTerm) {
        searchInput.value = searchTerm;
    }


    try {
        const result = await fetchApi(`/admin/deleted/comments?pageNumber=${page}&pageSize=10&searchTerm=${encodeURIComponent(searchTerm)}`, 'GET', null, true);
        renderDeletedCommentList(listContainer, result.items);
        renderPagination('pagination', result, loadDeletedComments);
    } catch (error) {
        if (error.status !== 401 && error.status !== 403) {
            if (listContainer) listContainer.innerHTML = `<p class="error-message">Failed to load deleted comments: ${error.message}</p>`;
        }
    }
}

function renderDeletedCommentList(container, comments) {
    if (!container) return;
    if (!comments || comments.length === 0) {
        container.innerHTML = '<p>Удаленные комментарии не найдены.</p>';
        return;
    }

    let commentsHtml = '';
    comments.forEach(comment => {
        commentsHtml += `
             <div class="deleted-item">
                 <div class="deleted-item-content">
                      <small>(From Post ID: ${comment.postId || 'N/A'})</small><br>
                     ${comment.content ? comment.content.replace(/\n/g, '<br>') : '<i>No content available</i>'}
                 </div>
                  <div class="deleted-item-header">
                     <span></span>
                     <div class="item-meta">
                         <span>Автор: ${comment.authorName || 'User Deleted/Unknown'} (ID: ${comment.authorId || 'N/A'})</span>
                         <span>Дата создания: ${formatDate(comment.createdDate)}</span>
                         <span>Дата удаления: ${formatDate(comment.deletedDate)}</span>
                     </div>
                 </div>
             </div>
         `;
    });
    container.innerHTML = commentsHtml;
}
