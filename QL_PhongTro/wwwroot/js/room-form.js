(() => {
    document.querySelectorAll("[data-vnd-input]").forEach(display => {
        const formatThousands = digits => digits.replace(/\B(?=(\d{3})+(?!\d))/g, ".");

        const updateValue = formatDisplay => {
            const raw = display.value.trim();
            const isPlainInteger = /^\d+$/.test(raw);
            const isGroupedInteger = /^\d{1,3}(?:\.\d{3})+$/.test(raw);
            const digits = isGroupedInteger ? raw.replaceAll(".", "") : isPlainInteger ? raw : "";
            if (formatDisplay && digits) display.value = formatThousands(digits);
        };

        display.addEventListener("input", () => updateValue(false));
        display.addEventListener("blur", () => updateValue(true));
        const initialValue = display.value.trim();
        if (/^\d+$/.test(initialValue)) display.value = formatThousands(initialValue);
    });
})();
(() => {
    const building = document.querySelector('[data-room-building]');
    const location = document.querySelector('[data-room-location]');
    if (!building || !location) return;
    const updateLocation = () => {
        location.textContent = building.selectedOptions[0]?.dataset.location || '';
    };
    building.addEventListener('change', updateLocation);
    updateLocation();
})();

// Trang trí lựa chọn cơ sở; select gốc vẫn giữ giá trị và validation của form.
(() => {
    const select = document.querySelector('.room-create-page [data-room-building], .room-create-page select#ToaNhaId');
    if (!select || select.disabled) return;
    const wrapper = document.createElement('div');
    wrapper.className = 'rc-building-picker';
    select.before(wrapper);
    wrapper.append(select);
    const trigger = document.createElement('button');
    trigger.type = 'button';
    trigger.className = 'rc-building-trigger';
    trigger.setAttribute('aria-haspopup', 'listbox');
    trigger.setAttribute('aria-labelledby', 'rc-building-label rc-building-value');
    const label = document.querySelector('label[for="' + select.id + '"]');
    if (label) label.id = 'rc-building-label';
    const value = document.createElement('span');
    value.id = 'rc-building-value';
    const prefix = document.createElement('span');
    prefix.className = 'rc-building-prefix';
    prefix.textContent = 'Cơ sở:';
    prefix.setAttribute('aria-hidden', 'true');
    trigger.append(prefix, value);
    const list = document.createElement('div');
    list.className = 'rc-building-options';
    list.id = 'rc-building-options';
    list.setAttribute('role', 'listbox');
    list.setAttribute('aria-label', 'Cơ sở');
    list.hidden = true;
    trigger.setAttribute('aria-controls', list.id);
    wrapper.append(trigger, list);
    const choices = Array.from(select.options).map(option => {
        const button = document.createElement('button');
        button.type = 'button';
        button.className = 'rc-building-option';
        button.setAttribute('role', 'option');
        button.textContent = option.textContent;
        button.disabled = option.disabled;
        button.addEventListener('click', () => {
            select.value = option.value;
            select.dispatchEvent(new Event('change', { bubbles: true }));
            setOpen(false);
            trigger.focus();
        });
        list.append(button);
        return button;
    });
    const sync = () => {
        value.textContent = select.selectedOptions[0]?.textContent || 'Chọn cơ sở';
        trigger.classList.toggle('is-placeholder', !select.value);
        choices.forEach((button, index) => button.setAttribute('aria-selected', String(index === select.selectedIndex)));
    };
    const setOpen = open => {
        list.hidden = !open;
        trigger.setAttribute('aria-expanded', String(open));
        if (open) choices[Math.max(0, select.selectedIndex)]?.focus();
    };
    trigger.addEventListener('click', () => setOpen(list.hidden));
    wrapper.addEventListener('keydown', event => {
        if (event.key === 'Escape') { setOpen(false); trigger.focus(); event.preventDefault(); }
        if (['ArrowDown', 'ArrowUp', 'Home', 'End'].includes(event.key)) {
            event.preventDefault();
            if (list.hidden) { setOpen(true); return; }
            const index = choices.indexOf(document.activeElement);
            const next = event.key === 'Home' ? 0 : event.key === 'End' ? choices.length - 1
                : Math.max(0, Math.min(choices.length - 1, index + (event.key === 'ArrowDown' ? 1 : -1)));
            choices[next]?.focus();
        }
    });
    wrapper.addEventListener('focusout', event => { if (!wrapper.contains(event.relatedTarget)) setOpen(false); });
    document.addEventListener('pointerdown', event => { if (!wrapper.contains(event.target)) setOpen(false); });
    select.addEventListener('change', sync);
    select.form?.addEventListener('reset', () => setTimeout(sync, 0));
    select.classList.add('rc-native-select');
    select.tabIndex = -1;
    select.setAttribute('aria-hidden', 'true');
    select.addEventListener('focus', () => trigger.focus());
    sync();
    setOpen(false);
})();

(() => {
    const building = document.querySelector('[data-room-building]');
    const suggestions = document.getElementById('rc-floor-options');
    if (!building || !suggestions) return;
    const updateFloors = () => {
        suggestions.replaceChildren();
        const count = Number(building.selectedOptions[0]?.dataset.floors);
        if (!Number.isSafeInteger(count) || count <= 0) return;
        for (let floor = 0; floor <= Math.min(count, 1000); floor++) {
            const option = document.createElement('option');
            option.value = String(floor);
            suggestions.append(option);
        }
    };
    building.addEventListener('change', updateFloors);
    updateFloors();
})();
