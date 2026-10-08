(() => {
    const page = document.querySelector('.service-create-page');
    if (!page) return;
    const get = id => page.querySelector('#' + id);
    const set = (key, value) => { const node = page.querySelector('[data-sc-' + key + ']'); if (node) node.textContent = value; };
    const update = () => {
        const type = page.querySelector('input[name="CachTinh"]:checked')?.value;
        const modes = {
            THEO_CHI_SO: ['Theo tiêu thụ', 'Thành tiền = (chỉ số cuối − chỉ số đầu) × đơn giá.', 'Ghi nhận chỉ số điện, nước mỗi kỳ và tính theo lượng tiêu thụ.'],
            THEO_NGUOI: ['Theo đầu người', 'Thành tiền = số người × đơn giá tháng.', 'Áp dụng theo số người sử dụng dịch vụ, thu cùng kỳ tiền thuê.'],
            CO_DINH: ['Cố định', 'Thành tiền = số phòng đăng ký × đơn giá tháng.', 'Áp dụng theo số phòng đăng ký; không phụ thuộc chỉ số công tơ.']
        };
        const mode = modes[type];
        set('name', get('TenDichVu')?.value.trim() || 'Chưa nhập tên dịch vụ');
        set('type', mode?.[0] || 'Chưa chọn cách tính');
        const raw = get('DonGia')?.value || '';
        set('price', /^\d+$/.test(raw) ? BigInt(raw).toLocaleString('vi-VN') + ' đ' : '— đ');
        set('unit', get('DonViTinh')?.value.trim() || '—');
        const building = get('ToaNhaId');
        set('building', building?.value ? building.selectedOptions[0].textContent : '—');
        set('scope', get('ApDungMacDinh')?.checked ? 'Mặc định cho phòng mới' : 'Chưa gán mặc định');
        set('formula', mode?.[1] || 'Chọn cách tính để xem công thức.');
        set('explanation', mode?.[2] || 'Chọn cách tính phí để xem giải thích tương ứng.');
    };
    page.addEventListener('input', update);
    page.addEventListener('change', update);
    update();
})();

(() => {
    const input = document.querySelector('.service-create-page #DonViTinh');
    const source = document.getElementById('sc-units');
    if (!input || !source) return;
    const wrapper = document.createElement('div');
    wrapper.className = 'sc-unit-picker';
    input.before(wrapper);
    wrapper.append(input);
    input.removeAttribute('list');
    input.setAttribute('role', 'combobox');
    input.setAttribute('aria-autocomplete', 'list');
    input.setAttribute('aria-controls', 'sc-unit-menu');
    const menu = document.createElement('div');
    menu.id = 'sc-unit-menu';
    menu.className = 'sc-unit-menu';
    menu.setAttribute('role', 'listbox');
    menu.setAttribute('aria-label', 'Đơn vị tính');
    menu.hidden = true;
    wrapper.append(menu);
    const choices = Array.from(source.options).map((option, index) => {
        const item = document.createElement('button');
        item.type = 'button';
        item.id = 'sc-unit-choice-' + index;
        item.setAttribute('role', 'option');
        item.tabIndex = -1;
        item.textContent = option.value;
        item.addEventListener('pointerdown', event => event.preventDefault());
        item.addEventListener('click', () => {
            input.value = option.value;
            input.dispatchEvent(new Event('input', { bubbles: true }));
            input.dispatchEvent(new Event('change', { bubbles: true }));
            toggle(false);
            input.focus();
        });
        menu.append(item);
        return item;
    });
    let active = -1;
    const sync = () => choices.forEach(item => item.setAttribute('aria-selected', String(item.textContent === input.value)));
    const toggle = open => {
        menu.hidden = !open;
        input.setAttribute('aria-expanded', String(open));
        active = -1;
        input.removeAttribute('aria-activedescendant');
        choices.forEach(item => item.classList.remove('is-active'));
        sync();
    };
    input.addEventListener('click', () => toggle(menu.hidden));
    input.addEventListener('input', () => toggle(true));
    input.addEventListener('keydown', event => {
        if (event.key === 'Escape') { toggle(false); event.preventDefault(); }
        if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
            event.preventDefault();
            if (menu.hidden) toggle(true);
            active = Math.max(0, Math.min(choices.length - 1, active + (event.key === 'ArrowDown' ? 1 : -1)));
            choices.forEach((item, index) => item.classList.toggle('is-active', index === active));
            input.setAttribute('aria-activedescendant', choices[active].id);
        }
        if (event.key === 'Enter' && !menu.hidden && active >= 0) { event.preventDefault(); choices[active].click(); }
        if (event.key === 'Tab') toggle(false);
    });
    input.addEventListener('blur', () => toggle(false));
    document.addEventListener('pointerdown', event => { if (!wrapper.contains(event.target)) toggle(false); });
    toggle(false);
})();

// Hiển thị tòa đang chọn dưới dạng thẻ; giữ select và giá trị gốc của form.
(() => {
    const select = document.querySelector('.service-create-page #ToaNhaId');
    if (!select) return;
    const wrapper = document.createElement('div');
    wrapper.className = 'sc-building-picker';
    select.before(wrapper);
    wrapper.append(select);
    const trigger = document.createElement('button');
    trigger.type = 'button';
    trigger.className = 'sc-building-trigger';
    trigger.setAttribute('aria-haspopup', 'listbox');
    trigger.setAttribute('aria-label', 'Chọn tòa nhà áp dụng');
    const chip = document.createElement('span');
    chip.className = 'sc-building-chip';
    trigger.append(chip);
    const menu = document.createElement('div');
    menu.className = 'sc-building-menu';
    menu.id = 'sc-building-menu';
    menu.setAttribute('role', 'listbox');
    menu.setAttribute('aria-label', 'Tòa nhà áp dụng');
    menu.hidden = true;
    trigger.setAttribute('aria-controls', menu.id);
    wrapper.append(trigger, menu);
    const items = Array.from(select.options).map(option => {
        const item = document.createElement('button');
        item.type = 'button';
        item.setAttribute('role', 'option');
        item.textContent = option.textContent;
        item.addEventListener('click', () => {
            select.value = option.value;
            select.dispatchEvent(new Event('change', { bubbles: true }));
            open(false);
            trigger.focus();
        });
        menu.append(item);
        return item;
    });
    const sync = () => {
        chip.textContent = select.selectedOptions[0]?.textContent || 'Chọn tòa nhà';
        chip.classList.toggle('is-placeholder', !select.value);
        items.forEach((item, index) => item.setAttribute('aria-selected', String(index === select.selectedIndex)));
    };
    const open = value => {
        menu.hidden = !value;
        trigger.setAttribute('aria-expanded', String(value));
        if (value) items[Math.max(0, select.selectedIndex)]?.focus();
    };
    trigger.addEventListener('click', () => open(menu.hidden));
    wrapper.addEventListener('keydown', event => {
        if (event.key === 'Escape') { open(false); trigger.focus(); event.preventDefault(); }
        if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
            event.preventDefault();
            if (menu.hidden) { open(true); return; }
            const next = Math.max(0, Math.min(items.length - 1, items.indexOf(document.activeElement) + (event.key === 'ArrowDown' ? 1 : -1)));
            items[next]?.focus();
        }
    });
    wrapper.addEventListener('focusout', event => { if (!wrapper.contains(event.relatedTarget)) open(false); });
    document.addEventListener('pointerdown', event => { if (!wrapper.contains(event.target)) open(false); });
    select.addEventListener('change', sync);
    select.classList.add('sc-building-native');
    select.tabIndex = -1;
    select.setAttribute('aria-hidden', 'true');
    select.addEventListener('focus', () => trigger.focus());
    sync();
    open(false);
})();
