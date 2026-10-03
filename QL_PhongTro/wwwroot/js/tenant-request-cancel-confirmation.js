(() => {
    const dialog = document.getElementById('tenant-request-cancel-confirmation');
    if (!dialog) return;

    let opener = null;
    document.querySelectorAll('[data-open-cancel-confirmation]').forEach(button => {
        button.addEventListener('click', () => {
            opener = button;
            dialog.showModal();
        });
    });

    dialog.addEventListener('close', () => {
        opener?.focus();
        opener = null;
    });
})();
