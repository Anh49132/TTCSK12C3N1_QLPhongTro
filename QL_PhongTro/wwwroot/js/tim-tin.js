(() => {
    const form = document.getElementById('tim-tin-form');
    const status = document.getElementById('tim-tin-loading');
    if (!form || !status) return;

    const showLoading = () => {
        status.hidden = false;
        document.querySelector('main')?.setAttribute('aria-busy', 'true');
    };
    form.addEventListener('submit', event => {
        if (!event.defaultPrevented && form.checkValidity()) showLoading();
    });
    document.querySelectorAll('nav[aria-label="Chuyển trang kết quả"] a, #tim-tin-price-suggestion').forEach(link => {
        link.addEventListener('click', event => {
            if (!event.defaultPrevented && event.button === 0 &&
                !event.ctrlKey && !event.metaKey && !event.shiftKey && !event.altKey) showLoading();
        });
    });
    // Back/forward may restore the old page from the browser cache.
    window.addEventListener('pageshow', () => {
        status.hidden = true;
        document.querySelector('main')?.removeAttribute('aria-busy');
    });
})();
