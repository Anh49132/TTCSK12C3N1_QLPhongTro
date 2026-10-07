function revealMeterTarget(target) {
    if (!target) return;
    target.focus({ preventScroll: true });
    target.scrollIntoView({ block: target.matches('[data-meter-input]') ? 'center' : 'start', behavior: 'auto' });
}

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
    form.querySelectorAll('[data-meter-input]').forEach(input => input.addEventListener('input', () => {
        const wasInvalid = input.getAttribute('aria-invalid') === 'true';
        if (!validate(input) && !wasInvalid) revealMeterTarget(input);
    }));
    // Use our field errors for submit too, instead of native validation hiding
    // another room's error above the current viewport. Constraints stay intact.
    form.noValidate = true;
    form.addEventListener('submit', event => {
        const inputs = [...form.querySelectorAll('[data-meter-input]')];
        const results = inputs.map(validate);
        if (results.includes(false)) {
            event.preventDefault();
            revealMeterTarget(inputs[results.indexOf(false)]);
        }
    });
});

const activeRoom = document.querySelector('[data-meter-room][data-meter-active="true"]');
if (activeRoom) {
    revealMeterTarget(activeRoom.querySelector('[data-meter-input][aria-invalid="true"]')
        || activeRoom.querySelector('[data-room-error]')
        || activeRoom.querySelector('[data-usage-warning]') || activeRoom);
} else if (/^#meter-room-\d+$/.test(window.location.hash)) {
    revealMeterTarget(document.getElementById(window.location.hash.slice(1)));
}
