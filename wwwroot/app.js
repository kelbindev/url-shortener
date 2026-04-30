window.urlShortener = (() => {
    const swal = () => Swal.mixin({
        background: 'rgba(23, 26, 33, 0.97)',
        color: '#c7d5e0',
    });

    return {
        shorten: async (url) => {
            const response = await fetch('/url', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json;charset=UTF-8' },
                body: JSON.stringify(url)
            });
            if (!response.ok) {
                if (response.status === 401)
                    throw new Error('Unauthorized! Click Get Token first to save your JWT token (this showcases JWT auth!)');
                if (response.status === 400)
                    throw new Error('URL is invalid');
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            const data = await response.json();
            return data.url;
        },

        getToken: async () => {
            const response = await fetch('/token', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json;charset=UTF-8' }
            });
            if (!response.ok)
                throw new Error(`HTTP error! status: ${response.status}`);
        },

        clearToken: async () => {
            const response = await fetch('/cleartoken', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json;charset=UTF-8' }
            });
            if (!response.ok)
                throw new Error(`HTTP error! status: ${response.status}`);
        },

        randomFact: async () => {
            const response = await fetch('https://uselessfacts.jsph.pl/api/v2/facts/random');
            if (!response.ok)
                throw new Error(`HTTP error! status: ${response.status}`);
            const data = await response.json();
            return data.text;
        },

        showError: (message) => swal().fire({ icon: 'error', html: message }),
        showSuccess: (title, message) => swal().fire({ icon: 'success', title, html: message }),
        showInfo: (title, message) => swal().fire({ icon: 'info', title, html: message }),
    };
})();
