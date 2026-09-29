document.addEventListener('click', event => {
    const button = event.target.closest('.toggle-password');
    if (!button) return;
    const input = document.getElementById(button.dataset.target);
    if (!input) return;
    const visible = input.type === 'text';
    input.type = visible ? 'password' : 'text';
    button.textContent = visible ? 'Hiện' : 'Ẩn';
});
