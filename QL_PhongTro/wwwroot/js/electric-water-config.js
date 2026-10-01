document.querySelectorAll('[data-utility]').forEach(section => {
    const method = section.querySelector('[data-method]');
    const refresh = () => section.querySelectorAll('[data-price]').forEach(group => {
        const active = group.dataset.price === method.value;
        group.hidden = !active;
        group.querySelectorAll('input').forEach(input => {
            input.disabled = !active;
            input.required = active;
        });
    });
    method.addEventListener('change', refresh);
    refresh();
});
