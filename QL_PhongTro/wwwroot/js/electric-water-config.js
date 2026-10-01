// Let the existing MVC unobtrusive validator show errors beside each price field.
// Disabled fields are excluded by both jQuery validation and form submission.
$(function () {
    document.querySelectorAll('[data-utility]').forEach(section => {
        const method = section.querySelector('[data-method]');
        section.querySelectorAll('[data-price] input').forEach(input => {
            $(input).rules('add', {
                min: 1,
                messages: { min: 'Mức giá phải là số nguyên đồng lớn hơn 0.' }
            });
        });
        const refresh = () => section.querySelectorAll('[data-price]').forEach(group => {
            const active = group.dataset.price === method.value;
            group.hidden = !active;
            group.querySelectorAll('input').forEach(input => {
                input.disabled = !active;
                input.required = active;
                if (!active) {
                    $(input).removeClass('input-validation-error').attr('aria-invalid', 'false');
                    $(section).find('[data-valmsg-for]').filter(function () {
                        return this.dataset.valmsgFor === input.name;
                    }).empty().removeClass('field-validation-error').addClass('field-validation-valid');
                }
            });
        });
        method.addEventListener('change', refresh);
        refresh();
    });
});
