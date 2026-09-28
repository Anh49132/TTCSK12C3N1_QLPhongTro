(function () {
    let isRefreshing = false;
    let failedQueue = [];
    let sessionRestored = false;

    const processQueue = (error, token = null) => {
        failedQueue.forEach(prom => {
            if (error) {
                prom.reject(error);
            } else {
                prom.resolve(token);
            }
        });
        failedQueue = [];
    };

    const refreshAccessToken = async () => {
        try {
            const refreshToken = localStorage.getItem('refreshToken');
            if (!refreshToken) throw new Error('No refresh token');

            const response = await fetch('/api/auth/refresh', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ RefreshToken: refreshToken })
            });

            if (!response.ok) throw new Error('Refresh failed');

            const data = await response.json();
            localStorage.setItem('accessToken', data.accessToken);
            localStorage.setItem('refreshToken', data.refreshToken);
            return data.accessToken;
        } catch (err) {
            localStorage.removeItem('accessToken');
            localStorage.removeItem('refreshToken');
            throw err;
        }
    };

    // Check if access token is expired (with 30 second buffer)
    const isAccessTokenExpired = () => {
        const accessToken = localStorage.getItem('accessToken');
        if (!accessToken) return true;
        try {
            const payload = JSON.parse(atob(accessToken.split('.')[1]));
            const exp = payload.exp * 1000; // convert to milliseconds
            return Date.now() >= (exp - 30000); // 30 second buffer
        } catch {
            return true;
        }
    };

    // Restore session on page load: if access token expired but refresh token exists, try to refresh
    const restoreSession = async () => {
        if (sessionRestored) return;
        sessionRestored = true;

        const accessToken = localStorage.getItem('accessToken');
        const refreshToken = localStorage.getItem('refreshToken');

        if (!accessToken || !refreshToken) return;

        if (isAccessTokenExpired()) {
            try {
                await refreshAccessToken();
                console.log('Session restored via refresh token');
            } catch (err) {
                console.warn('Session restore failed, tokens cleared');
                localStorage.removeItem('accessToken');
                localStorage.removeItem('refreshToken');
                window.dispatchEvent(new Event('authChanged'));
            }
        }
    };

    // Call restore on page load
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', restoreSession);
    } else {
        restoreSession();
    }

    const originalFetch = window.fetch;
    window.fetch = async (url, options = {}) => {
        const accessToken = localStorage.getItem('accessToken');
        if (accessToken && !options.headers?.Authorization) {
            options.headers = {
                ...options.headers,
                'Authorization': `Bearer ${accessToken}`
            };
        }

        let response = await originalFetch(url, options);

        if (response.status === 401) {
            const originalUrl = url;
            const originalOptions = { ...options };

            if (!isRefreshing) {
                isRefreshing = true;
                try {
                    const newAccessToken = await refreshAccessToken();
                    isRefreshing = false;
                    processQueue(null, newAccessToken);
                } catch (err) {
                    isRefreshing = false;
                    processQueue(err);
                    localStorage.removeItem('accessToken');
                    localStorage.removeItem('refreshToken');
                    window.dispatchEvent(new Event('authChanged'));
                    window.location.href = '/Account/Login';
                    throw err;
                }
            } else {
                try {
                    const newAccessToken = await new Promise((resolve, reject) => {
                        failedQueue.push({ resolve, reject });
                    });
                    options.headers = {
                        ...options.headers,
                        'Authorization': `Bearer ${newAccessToken}`
                    };
                    return originalFetch(originalUrl, originalOptions);
                } catch (err) {
                    throw err;
                }
            }

            options.headers = {
                ...options.headers,
                'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
            };
            return originalFetch(url, options);
        }

        return response;
    };

    window.authSetTokens = (accessToken, refreshToken) => {
        localStorage.setItem('accessToken', accessToken);
        localStorage.setItem('refreshToken', refreshToken);
        window.dispatchEvent(new Event('authChanged'));
    };

    window.authClearTokens = () => {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        window.dispatchEvent(new Event('authChanged'));
    };
})();