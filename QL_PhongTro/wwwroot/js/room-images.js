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
    let isDeletingImage = false;
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
            card.draggable = !touch && !isSavingOrder && !isDeletingImage;
            const order = index + 1;
            const label = card.querySelector('[data-order-label]');
            const image = card.querySelector('img');
            const primary = card.querySelector('[data-primary-label]');
            const up = card.querySelector('[data-move-image="up"]');
            const down = card.querySelector('[data-move-image="down"]');
            const remove = card.querySelector('[data-delete-image]');
            if (label) label.textContent = `Ảnh ${order}`;
            if (image) image.alt = `Ảnh phòng thứ ${order}`;
            if (primary) primary.hidden = index !== 0;
            if (up) up.disabled = index === 0 || isSavingOrder || isDeletingImage;
            if (down) down.disabled = index === cards.length - 1 || isSavingOrder || isDeletingImage;
            if (remove) remove.disabled = isSavingOrder || isDeletingImage;
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
        const remove = document.createElement('button');
        remove.type = 'button';
        remove.className = 'room-image-delete-button';
        remove.dataset.deleteImage = '';
        remove.setAttribute('aria-label', 'Xóa ảnh');
        remove.title = 'Xóa ảnh';
        remove.textContent = 'Xóa';
        controls.append(remove);
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

    function confirmDelete() {
        return new Promise(resolve => {
            const dialog = document.createElement('dialog');
            dialog.className = 'room-image-confirm';
            dialog.innerHTML = `
                <form method="dialog">
                    <p>Xoá ảnh này? Ảnh sẽ bị xoá vĩnh viễn và không thể khôi phục.</p>
                    <div>
                        <button value="cancel">Huỷ</button>
                        <button value="delete" class="room-image-confirm-delete">Xoá</button>
                    </div>
                </form>`;
            document.body.append(dialog);
            dialog.addEventListener('close', () => {
                const accepted = dialog.returnValue === 'delete';
                dialog.remove();
                resolve(accepted);
            }, { once: true });
            if (typeof dialog.showModal === 'function') {
                dialog.showModal();
            } else {
                const accepted = window.confirm('Xoá ảnh này? Ảnh sẽ bị xoá vĩnh viễn và không thể khôi phục.');
                dialog.remove();
                resolve(accepted);
            }
        });
    }

    function applyServerImages(images) {
        const ids = new Set((images || []).map(image => Number(image.id)));
        imageCards().forEach(card => {
            if (!ids.has(Number(card.dataset.imageId))) card.remove();
        });
        arrangeCards((images || []).map(image => Number(image.id)));
        count = ids.size;
        committedOrder = imageIds();
        updateCount();
        updateOrderPresentation();
    }

    function deleteImage(card) {
        if (!manager.dataset.deleteUrlTemplate || isSavingOrder || isDeletingImage) return;
        const imageId = Number(card.dataset.imageId);
        if (!imageId) return;
        confirmDelete().then(accepted => {
            if (!accepted) return;
            isDeletingImage = true;
            sortStatus.textContent = 'Đang xoá ảnh...';
            sortStatus.classList.remove('is-error');
            updateOrderPresentation();

            const request = new XMLHttpRequest();
            request.open('POST', manager.dataset.deleteUrlTemplate.replace('__imageId__', encodeURIComponent(imageId)));
            request.setRequestHeader('RequestVerificationToken', token);
            request.onload = () => {
                let response = {};
                try { response = JSON.parse(request.responseText); } catch { }
                if (request.status >= 200 && request.status < 300) {
                    applyServerImages(response.images || []);
                    sortStatus.textContent = 'Đã xoá ảnh.';
                } else {
                    sortStatus.textContent = response.message || 'Không xoá được ảnh. Vui lòng thử lại.';
                    sortStatus.classList.add('is-error');
                }
                isDeletingImage = false;
                updateOrderPresentation();
            };
            request.onerror = () => {
                sortStatus.textContent = 'Mất kết nối khi xoá ảnh. Ảnh vẫn được giữ nguyên.';
                sortStatus.classList.add('is-error');
                isDeletingImage = false;
                updateOrderPresentation();
            };
            request.send();
        });
    }

    grid.addEventListener('click', event => {
        const deleteButton = event.target.closest('[data-delete-image]');
        if (deleteButton) {
            const card = deleteButton.closest('.room-image-card[data-image-id]');
            if (card) deleteImage(card);
            return;
        }
        const button = event.target.closest('[data-move-image]');
        if (!button || isSavingOrder || isDeletingImage) return;
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
        if (!card || isSavingOrder || isDeletingImage || window.matchMedia('(pointer: coarse)').matches) {
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
        if (!draggedCard || isSavingOrder || isDeletingImage) return;
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
