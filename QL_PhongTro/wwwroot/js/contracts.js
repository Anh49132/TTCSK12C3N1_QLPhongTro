(() => {
    const start = document.getElementById('NgayBatDau');
    const months = document.getElementById('SoThang');
    const end = document.getElementById('end-date');
    if (!start || !months || !end) return;
    const display = value => value ? value.split('-').reverse().join('/') : '—';
    function update() {
        end.value = '';
        const parts = start.value.split('-').map(Number), count = Number(months.value);
        if (parts.length === 3 && Number.isInteger(count) && count >= 1 && count <= 120 && parts[0] <= 9988) {
            const date = new Date(Date.UTC(parts[0], parts[1] - 1 + count, 1));
            const last = new Date(Date.UTC(date.getUTCFullYear(), date.getUTCMonth() + 1, 0)).getUTCDate();
            date.setUTCDate(Math.min(parts[2], last));
            date.setUTCDate(date.getUTCDate() - 1);
            end.value = date.toISOString().slice(0, 10);
        }
        document.getElementById('start-preview').textContent = display(start.value);
        document.getElementById('end-preview').textContent = display(end.value);
    }
    start.addEventListener('input', update); months.addEventListener('input', update);
    const rent = document.getElementById('GiaThue');
    const deposit = document.getElementById('TienCoc');
    let depositEdited = false;
    function updateDeposit() {
        if (/^\d+$/.test(rent.value)) {
            deposit.max = (BigInt(rent.value) * 3n).toString();
            if (!depositEdited) deposit.value = rent.value;
        } else deposit.removeAttribute('max');
        const preview = document.getElementById('deposit-preview');
        if (preview) preview.textContent = /^\d+$/.test(deposit.value) ? BigInt(deposit.value).toLocaleString('vi-VN') + ' đ' : '—';
        const rentPreview = document.getElementById('rent-preview');
        if (rentPreview) rentPreview.textContent = /^\d+$/.test(rent.value) ? BigInt(rent.value).toLocaleString('vi-VN') + ' đ/tháng' : '—';
    }
    deposit.addEventListener('input', () => { depositEdited = true; updateDeposit(); });
    rent.addEventListener('input', updateDeposit);
    // Preserve values of existing drafts and failed submissions on initial load.
    depositEdited = true; updateDeposit(); depositEdited = deposit.value !== rent.value;
    const status = document.getElementById('overlap-status'), save = document.getElementById('save-contract');
    const request = document.getElementById('YeuCauId').value;
    let timer, abort;
    async function checkOverlap() {
        if (abort) abort.abort();
        if (!request || !start.value || !start.validity.valid || !months.validity.valid || !end.value) {
            save.disabled = true;
            status.textContent = 'Vui lòng chọn ngày bắt đầu từ hôm nay và kỳ hạn hợp lệ.';
            return;
        }
        abort = new AbortController();
        status.textContent = 'Đang kiểm tra khoảng thời gian thuê…';
        save.disabled = true;
        try {
            const query = new URLSearchParams({ yeuCauId: request, ngayBatDau: start.value, soThang: months.value });
            const response = await fetch('/HopDong/KiemTra?' + query, { signal: abort.signal });
            if (!response.ok) throw new Error('Không thể kiểm tra');
            const conflicts = await response.json();
            status.replaceChildren();
            status.classList.toggle('has-conflict', conflicts.length > 0);
            const title = document.createElement('strong');
            title.textContent = conflicts.length ? 'Không thể lưu — có hợp đồng chồng lấn:' : 'Không có hợp đồng hiệu lực chồng lấn trong khoảng thời gian đã chọn.';
            status.append(title);
            if (conflicts.length) {
                const list = document.createElement('ul');
                for (const item of conflicts) { const li = document.createElement('li'); li.textContent = `${item.ma}: ${item.tuNgay} – ${item.denNgay}`; list.append(li); }
                status.append(list);
            }
            save.disabled = conflicts.length > 0;
        } catch (error) {
            if (error.name === 'AbortError') return;
            status.textContent = 'Chưa kiểm tra được. Hệ thống sẽ kiểm tra lại trên máy chủ khi lưu.';
            save.disabled = false;
        }
    }
    function scheduleCheck() { if (abort) abort.abort(); clearTimeout(timer); timer = setTimeout(checkOverlap, 250); }
    start.addEventListener('input', scheduleCheck); months.addEventListener('input', scheduleCheck);
    if (request) checkOverlap();
})();
