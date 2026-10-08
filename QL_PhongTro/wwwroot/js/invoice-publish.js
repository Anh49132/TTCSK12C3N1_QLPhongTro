(() => {
    const editor = document.getElementById('draft-editor');
    const dialog = document.getElementById('publish-confirmation');
    const open = document.getElementById('publish-open');
    const form = document.getElementById('publish-draft-form');
    if (!editor || !dialog || !open || !form) return;
    let dirty = false;
    const changed = () => {
        dirty = true;
        open.disabled = true;
        document.getElementById('publish-dirty').hidden = false;
    };
    editor.addEventListener('input', changed);
    editor.addEventListener('change', changed);
    open.addEventListener('click', () => { if (!dirty) dialog.showModal(); });
    document.getElementById('publish-close').addEventListener('click', () => dialog.close());
    const date = document.getElementById('publish-date');
    const due = document.getElementById('publish-due');
    date.addEventListener('change', () => { due.min = date.value; });
    due.min = date.value;
    form.addEventListener('submit', event => {
        if (dirty) { event.preventDefault(); dialog.close(); return; }
        form.querySelector('button[type="submit"]').disabled = true;
    });
})();
