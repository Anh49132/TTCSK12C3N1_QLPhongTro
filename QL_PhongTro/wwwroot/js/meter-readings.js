document.querySelectorAll('[data-meter-form]').forEach(form => {
    function validate(input) {
        const previous = input.dataset.previous;
        const value = input.value;
        let error = '';
        if (previous == null || previous === '') error = 'Chưa có dữ liệu tham chiếu. Không thể chốt chỉ số.';
        else if (!value || !input.validity.valid || !/^\d+(?:\.\d{1,3})?$/.test(value))
            error = 'Nhập chỉ số không âm, tối đa 3 chữ số thập phân.';
        else if (Number(value) < Number(previous)) error = 'Chỉ số mới không được nhỏ hơn chỉ số kỳ trước.';
        input.classList.toggle('is-invalid', Boolean(error));
        input.setAttribute('aria-invalid', error ? 'true' : 'false');
        document.getElementById(input.id + '-error').textContent = error;
        return !error;
    }
    form.querySelectorAll('[data-meter-input]').forEach(input => input.addEventListener('input', () => validate(input)));
    form.addEventListener('submit', event => {
        const results = [...form.querySelectorAll('[data-meter-input]')].map(validate);
        if (results.includes(false)) event.preventDefault();
    });
});
