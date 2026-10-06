(() => {
    const panel = document.querySelector('.building-list-panel');
    if (!panel) return;
    const rows = [...panel.querySelectorAll('tr[data-building-id]')];
    const search = panel.querySelector('#building-search');
    const area = panel.querySelector('#building-area');
    const status = panel.querySelector('#building-status');
    const sort = panel.querySelector('.building-sort');
    const normalize = value => value.normalize('NFD').replace(/[\u0300-\u036f]/g, '').replace(/đ/g, 'd').toLocaleLowerCase('vi');
    const filter = () => {
        rows.forEach(row => row.hidden = !normalize(row.dataset.search).includes(normalize(search.value.trim())) ||
            (area.value && row.dataset.area !== area.value) || (status.value && row.dataset.status !== status.value));
        panel.querySelector('.building-filter-empty').hidden = !rows.length || rows.some(row => !row.hidden);
    };
    [search, area, status].forEach(input => input.addEventListener('input', filter));
    const order = () => {
        const ordered = [...rows].sort((a, b) => sort.dataset.sort === 'newest' ? Number(b.dataset.buildingId) - Number(a.dataset.buildingId) : a.dataset.search.localeCompare(b.dataset.search, 'vi'));
        ordered.forEach(row => row.parentElement.append(row));
    };
    sort.addEventListener('click', () => {
        sort.dataset.sort = sort.dataset.sort === 'newest' ? 'name' : 'newest';
        sort.querySelector('span').textContent = sort.dataset.sort === 'newest' ? 'Mới nhất' : 'Tên A–Z';
        order();
    });
    panel.querySelectorAll('[data-view]').forEach(button => button.addEventListener('click', () => {
        panel.classList.toggle('building-grid-view', button.dataset.view === 'grid');
        panel.querySelectorAll('[data-view]').forEach(item => item.setAttribute('aria-pressed', String(item === button)));
    }));
    filter(); order();
})();
