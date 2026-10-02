// S2-08 lát 2: cảnh báo lịch hẹn trùng trong khoảng ±30 phút.
// Cảnh báo chỉ thông tin; chủ nhà vẫn xác nhận được.
(function () {
    const picker = document.querySelector('[data-role="lich-hen"]');
    const warning = document.querySelector('[data-role="canh-bao-trung"]');
    if (!picker || !warning) return;

    const yeuCauId = warning.dataset.yeuCauId;
    if (!yeuCauId) return;

    const defaultText = warning.textContent;
    let lastChecked = null;

    function render(list) {
        warning.classList.remove('text-danger');
        if (!list || list.length === 0) {
            warning.textContent = defaultText;
            return;
        }
        const items = list.map(x => `${x.lichHenHienThoi} — ${x.tenPhong}, khách ${x.tenKhach} (${x.maYeuCau})`);
        warning.classList.add('text-danger');
        warning.textContent = `Cảnh báo trùng lịch: phòng đã có ${list.length} lịch hẹn khác trong khoảng ±30 phút: `
            + items.join('; ') + '. Bạn vẫn có thể xác nhận.';
    }

    async function check() {
        const value = picker.value;
        if (!value) { render([]); return; }
        if (value === lastChecked) return;
        lastChecked = value;
        try {
            const url = `/LichHen/LichTrung?id=${encodeURIComponent(yeuCauId)}`
                + `&lichHen=${encodeURIComponent(value)}`;
            const response = await fetch(url, { headers: { 'Accept': 'application/json' } });
            if (!response.ok) { render([]); return; }
            render(await response.json());
        } catch {
            // A failed advisory lookup must never block confirming the slot.
            render([]);
        }
    }

    picker.addEventListener('change', check);
    check();
})();
