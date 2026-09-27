(function () {
    let isRefreshing = false;
    let failedQueue = [];

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
                    // Redirect to login
                    localStorage.removeItem('accessToken');
                    localStorage.removeItem('refreshToken');
                    window.location.href = '/Account/Login';
                    throw err;
                }
            } else {
                // Wait for refresh to complete
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

            // Retry original request with new token
            options.headers = {
                ...options.headers,
                'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
            };
            return originalFetch(url, options);
        }

        return response;
    };

    // Helper to set tokens after login
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