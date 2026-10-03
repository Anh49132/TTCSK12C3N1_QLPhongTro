(() => {
    const dialog = document.getElementById('tenant-request-cancel-confirmation');
    if (!dialog) return;

    let opener = null;
    let pendingForm = null;
    document.querySelectorAll('[data-open-cancel-confirmation]').forEach(button => {
        button.addEventListener('click', () => {
            opener = button;
            pendingForm = button.closest('form[data-cancel-request]');
            if (!pendingForm) return;
            dialog.returnValue = 'cancel';
            dialog.showModal();
        });
    });

    dialog.addEventListener('close', () => {
        const form = pendingForm;
        pendingForm = null;
        opener?.focus();
        opener = null;
        if (dialog.returnValue === 'confirm' && form) {
            form.requestSubmit();
        }
    });
})();
