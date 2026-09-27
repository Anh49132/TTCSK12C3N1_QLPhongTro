$(function () {
    $('#SoCanCuoc').on('input blur', function () {
        $(this).valid();
    });
    [['AnhMatTruoc', 'preview-truoc', 'anh-truoc-error'], ['AnhMatSau', 'preview-sau', 'anh-sau-error']].forEach(function (ids) {
        const input = document.getElementById(ids[0]);
        const preview = document.getElementById(ids[1]);
        const error = document.getElementById(ids[2]);
        const savedSource = preview.getAttribute('src');
        let objectUrl;
        input.addEventListener('change', function () {
            if (objectUrl) URL.revokeObjectURL(objectUrl);
            preview.hidden = !savedSource;
            if (savedSource) preview.src = savedSource;
            else preview.removeAttribute('src');
            const file = input.files[0];
            let message = '';
            if (file && !/\.(jpe?g|png)$/i.test(file.name)) message = 'Chỉ chấp nhận ảnh JPG hoặc PNG.';
            else if (file && file.size > 5 * 1024 * 1024) message = 'Mỗi ảnh không được vượt quá 5MB.';
            error.textContent = message;
            input.setCustomValidity(message);
            input.setAttribute('aria-invalid', message ? 'true' : 'false');
            if (file && !message) {
                objectUrl = URL.createObjectURL(file);
                preview.src = objectUrl;
                preview.hidden = false;
            }
        });
    });
});
