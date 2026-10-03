(() => {
    const manager = document.querySelector('.room-image-manager');
    const input = document.getElementById('room-image-files');
    const grid = manager?.querySelector('[data-image-grid]');
    const queue = manager?.querySelector('[data-image-queue]');
    const countElement = manager?.querySelector('[data-image-count]');
    const sortStatus = manager?.querySelector('[data-sort-status]');
    const form = manager?.closest('form');
    const token = form?.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (!manager || !input || !grid || !queue || !countElement || !sortStatus || !token) return;

    let count = grid.querySelectorAll('[data-image-id]').length;
    let committedOrder = [];
    let draggedCard = null;
    let orderAtDragStart = null;
    let isSavingOrder = false;
    const updateCount = () => { countElement.textContent = `${count}/8 ảnh`; };

    function imageCards() {
        return Array.from(grid.querySelectorAll('.room-image-card[data-image-id]'));
    }

    function imageIds() {
        return imageCards().map(card => Number(card.dataset.imageId));
    }

    function arrangeCards(ids) {
        const byId = new Map(imageCards().map(card => [Number(card.dataset.imageId), card]));
        ids.forEach(id => {
            const card = byId.get(id);
            if (card) grid.append(card);
        });
    }

    function updateOrderPresentation() {
        const touch = window.matchMedia('(pointer: coarse)').matches;
        imageCards().forEach((card, index, cards) => {
            card.draggable = !touch && !isSavingOrder;
            const order = index + 1;
            const label = card.querySelector('[data-order-label]');
            const image = card.querySelector('img');
            const primary = card.querySelector('[data-primary-label]');
            const up = card.querySelector('[data-move-image="up"]');
            const down = card.querySelector('[data-move-image="down"]');
            if (label) label.textContent = `Ảnh ${order}`;
            if (image) image.alt = `Ảnh phòng thứ ${order}`;
            if (primary) primary.hidden = index !== 0;
            if (up) up.disabled = index === 0 || isSavingOrder;
            if (down) down.disabled = index === cards.length - 1 || isSavingOrder;
        });
    }

    function addImageControls(card) {
        const footer = document.createElement('div');
        footer.className = 'room-image-card-footer';
        const label = document.createElement('span');
        label.dataset.orderLabel = '';
        const controls = document.createElement('div');
        controls.className = 'room-image-move-controls';
        controls.setAttribute('aria-label', 'Sắp xếp ảnh');
        for (const [direction, icon, description] of [
            ['up', '↑', 'Đưa ảnh lên trước'],
            ['down', '↓', 'Đưa ảnh xuống sau']
        ]) {
            const button = document.createElement('button');
            button.type = 'button';
            button.dataset.moveImage = direction;
            button.setAttribute('aria-label', description);
            button.title = description;
            button.textContent = icon;
            controls.append(button);
        }
        footer.append(label, controls);
        const primary = document.createElement('span');
        primary.className = 'room-image-primary';
        primary.dataset.primaryLabel = '';
        primary.hidden = true;
        primary.textContent = 'Ảnh đại diện';
        card.querySelector('.room-image-preview')?.append(primary);
        card.append(footer);
    }

    function saveCurrentOrder(previousOrder) {
        const nextOrder = imageIds();
        if (nextOrder.length < 2 || nextOrder.every((id, index) => id === previousOrder[index])) {
            updateOrderPresentation();
            return;
        }

        isSavingOrder = true;
        sortStatus.textContent = 'Đang lưu thứ tự ảnh...';
        sortStatus.classList.remove('is-error');
        updateOrderPresentation();

        const request = new XMLHttpRequest();
        request.open('POST', manager.dataset.reorderUrl);
        request.setRequestHeader('Content-Type', 'application/json');
        request.setRequestHeader('RequestVerificationToken', token);
        request.onload = () => {
            let response = {};
            try { response = JSON.parse(request.responseText); } catch { }
            if (request.status >= 200 && request.status < 300) {
                committedOrder = nextOrder;
                sortStatus.textContent = 'Đã lưu thứ tự ảnh.';
            } else {
                arrangeCards(previousOrder);
                committedOrder = previousOrder;
                sortStatus.textContent = response.message || 'Không lưu được thứ tự ảnh. Đã khôi phục thứ tự cũ.';
                sortStatus.classList.add('is-error');
            }
            isSavingOrder = false;
            updateOrderPresentation();
        };
        request.onerror = () => {
            arrangeCards(previousOrder);
            committedOrder = previousOrder;
            sortStatus.textContent = 'Mất kết nối khi lưu thứ tự. Đã khôi phục thứ tự cũ.';
            sortStatus.classList.add('is-error');
            isSavingOrder = false;
            updateOrderPresentation();
        };
        request.send(JSON.stringify(nextOrder));
    }

    committedOrder = imageIds();
    updateOrderPresentation();

    grid.addEventListener('click', event => {
        const button = event.target.closest('[data-move-image]');
        if (!button || isSavingOrder) return;
        const card = button.closest('.room-image-card[data-image-id]');
        const cards = imageCards();
        const index = cards.indexOf(card);
        const direction = button.dataset.moveImage === 'up' ? -1 : 1;
        const target = cards[index + direction];
        if (!target) return;
        const previousOrder = imageIds();
        if (direction < 0) grid.insertBefore(card, target);
        else grid.insertBefore(target, card);
        updateOrderPresentation();
        saveCurrentOrder(previousOrder);
    });

    grid.addEventListener('dragstart', event => {
        const card = event.target.closest('.room-image-card[data-image-id]');
        if (!card || isSavingOrder || window.matchMedia('(pointer: coarse)').matches) {
            event.preventDefault();
            return;
        }
        draggedCard = card;
        orderAtDragStart = imageIds();
        card.classList.add('is-dragging');
        event.dataTransfer.effectAllowed = 'move';
        event.dataTransfer.setData('text/plain', card.dataset.imageId);
    });

    grid.addEventListener('dragover', event => {
        if (!draggedCard || isSavingOrder) return;
        const target = event.target.closest('.room-image-card[data-image-id]');
        if (!target || target === draggedCard) return;
        event.preventDefault();
        const bounds = target.getBoundingClientRect();
        const after = event.clientY > bounds.top + bounds.height / 2
            || (Math.abs(event.clientY - (bounds.top + bounds.height / 2)) < bounds.height / 4
                && event.clientX > bounds.left + bounds.width / 2);
        target.classList.add('is-drag-target');
        grid.insertBefore(draggedCard, after ? target.nextSibling : target);
    });

    grid.addEventListener('dragleave', event => {
        event.target.closest('.room-image-card')?.classList.remove('is-drag-target');
    });

    grid.addEventListener('drop', event => {
        if (!draggedCard) return;
        event.preventDefault();
        const previousOrder = orderAtDragStart || committedOrder;
        draggedCard.classList.remove('is-dragging');
        grid.querySelectorAll('.is-drag-target').forEach(card => card.classList.remove('is-drag-target'));
        draggedCard = null;
        orderAtDragStart = null;
        updateOrderPresentation();
        saveCurrentOrder(previousOrder);
    });

    grid.addEventListener('dragend', () => {
        if (draggedCard && orderAtDragStart) arrangeCards(orderAtDragStart);
        draggedCard?.classList.remove('is-dragging');
        draggedCard = null;
        orderAtDragStart = null;
        grid.querySelectorAll('.is-drag-target').forEach(card => card.classList.remove('is-drag-target'));
        updateOrderPresentation();
    });

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
                    view.preview.replaceChildren(image);
                    view.progressBar.style.width = '100%';
                    view.label.remove();
                    view.message.remove();
                    view.card.dataset.imageId = response.id;
                    view.card.removeChild(view.card.querySelector('.room-image-progress'));
                    addImageControls(view.card);
                    grid.append(view.card);
                    count = response.count;
                    committedOrder = imageIds();
                    updateCount();
                    updateOrderPresentation();
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