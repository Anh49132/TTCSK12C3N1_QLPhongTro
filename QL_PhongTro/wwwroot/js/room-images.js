(() => {
    const manager = document.querySelector('.room-image-manager');
    const input = document.getElementById('room-image-files');
    const grid = manager?.querySelector('[data-image-grid]');
    const queue = manager?.querySelector('[data-image-queue]');
    const countElement = manager?.querySelector('[data-image-count]');
    const form = manager?.closest('form');
    const token = form?.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (!manager || !input || !grid || !queue || !countElement || !token) return;

    let count = grid.querySelectorAll('[data-image-id]').length;
    const updateCount = () => { countElement.textContent = `${count}/8 ảnh`; };

    function makeCard(file) {
        const card = document.createElement('article');
        card.className = 'room-image-card';
        const preview = document.createElement('div');
        preview.className = 'room-image-preview';
        const progress = document.createElement('div');
        progress.className = 'room-image-progress';
        const progressBar = document.createElement('span');
        progress.append(progressBar);
        const label = document.createElement('span');
        label.textContent = file.name;
        const message = document.createElement('p');
        message.setAttribute('role', 'status');
        card.append(preview, progress, label, message);
        queue.append(card);
        return { card, preview, progressBar, label, message };
    }

    function upload(file, view) {
        return new Promise(resolve => {
            const request = new XMLHttpRequest();
            const body = new FormData();
            body.append('file', file, file.name);
            body.append('__RequestVerificationToken', token);
            request.open('POST', manager.dataset.uploadUrl);
            request.upload.onprogress = event => {
                if (event.lengthComputable)
                    view.progressBar.style.width = `${Math.round(event.loaded / event.total * 100)}%`;
            };
            request.onload = () => {
                let response = {};
                try { response = JSON.parse(request.responseText); } catch { }
                if (request.status >= 200 && request.status < 300 && response.thumbnailPath && response.id) {
                    const image = document.createElement('img');
                    image.src = response.thumbnailPath;
                    image.alt = `Ảnh phòng thứ ${response.order}`;
                    view.preview.replaceWith(image);
                    view.progressBar.style.width = '100%';
                    view.label.textContent = `Ảnh ${response.order}`;
                    view.message.remove();
                    view.card.dataset.imageId = response.id;
                    view.card.removeChild(view.card.querySelector('.room-image-progress'));
                    grid.append(view.card);
                    count = response.count;
                    updateCount();
                } else {
                    view.card.classList.add('is-failed');
                    view.progressBar.style.width = '100%';
                    view.message.textContent = response.message || 'Không tải được ảnh. Vui lòng kiểm tra kết nối mạng và thử lại.';
                }
                resolve();
            };
            request.onerror = () => {
                view.card.classList.add('is-failed');
                view.message.textContent = 'Mất kết nối khi tải ảnh. Ảnh chưa được lưu; vui lòng thử lại.';
                resolve();
            };
            request.onabort = () => {
                view.card.classList.add('is-failed');
                view.message.textContent = 'Đã hủy tải ảnh. Ảnh chưa được lưu.';
                resolve();
            };
            request.send(body);
        });
    }

    input.addEventListener('change', async () => {
        const files = Array.from(input.files || []);
        input.value = '';
        for (const file of files) {
            const view = makeCard(file);
            await upload(file, view);
        }
    });

    updateCount();
})();