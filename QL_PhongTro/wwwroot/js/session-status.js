(() => {
    async function checkSession() {
        try {
            const response = await fetch('/Account/SessionStatus', { cache: 'no-store', credentials: 'same-origin' });
            if (!response.ok) return;
            const state = await response.json();
            if (!state.authenticated) window.location.replace('/Account/Login');
            else if (state.mustChangePassword && !location.pathname.toLowerCase().startsWith('/account/changepassword'))
                window.location.replace('/Account/ChangePassword');
        } catch { /* The server still checks authorization on every data request. */ }
    }
    setInterval(checkSession, 20000);
    window.addEventListener('focus', checkSession);
})();
