window.showToast = function (message, type = "success") {
    const host = document.getElementById("toast-host");

    if (!host) {
        return;
    }

    const toast = document.createElement("div");
    toast.className = `app-toast ${type === "error" ? "error" : "success"}`;
    toast.setAttribute("role", type === "error" ? "alert" : "status");

    const icon = type === "error" ? "!" : "✓";
    toast.innerHTML = `
        <span class="app-toast-icon" aria-hidden="true">${icon}</span>
        <span class="app-toast-message">${message}</span>
    `;

    host.appendChild(toast);

    window.setTimeout(() => {
        toast.classList.add("leaving");
        toast.addEventListener("transitionend", () => toast.remove(), { once: true });
    }, 4200);
};

(function () {
    const nativeFetch = window.fetch.bind(window);
    let redirectingToLogin = false;

    function requestPath(input) {
        const rawUrl = typeof input === "string" ? input : input?.url || "";

        try {
            return new URL(rawUrl, window.location.origin).pathname.toLowerCase();
        } catch {
            return "";
        }
    }

    function shouldRedirectToLogin(input, response) {
        if (response.status !== 401 || redirectingToLogin) {
            return false;
        }

        const path = requestPath(input);
        return path.startsWith("/api/") && !path.startsWith("/api/auth/");
    }

    function redirectToLogin() {
        redirectingToLogin = true;

        if (window.location.pathname !== "/") {
            window.showToast?.("La sesion caduco. Inicie sesion nuevamente.", "error");
        }

        window.setTimeout(() => {
            const target = window.top || window;
            target.location.href = "/";
        }, 600);
    }

    window.fetch = async function (input, init) {
        const response = await nativeFetch(input, init);

        if (shouldRedirectToLogin(input, response)) {
            redirectToLogin();
        }

        return response;
    };
})();

let appLoadingCount = 0;

function ensureAppLoadingOverlay() {
    let overlay = document.getElementById("app-loading-overlay");

    if (overlay) {
        return overlay;
    }

    overlay = document.createElement("div");
    overlay.id = "app-loading-overlay";
    overlay.className = "app-loading-overlay";
    overlay.setAttribute("role", "status");
    overlay.setAttribute("aria-live", "polite");
    overlay.setAttribute("aria-label", "Cargando");
    overlay.innerHTML = `
        <div class="app-loading-card">
            <span class="app-loading-spinner" aria-hidden="true"></span>
        </div>
    `;
    document.body.appendChild(overlay);
    return overlay;
}

window.showAppLoading = function (message = "Procesando...") {
    appLoadingCount += 1;
    const overlay = ensureAppLoadingOverlay();

    overlay.classList.add("is-visible");
    document.body.classList.add("is-app-loading");
};

window.hideAppLoading = function () {
    appLoadingCount = Math.max(appLoadingCount - 1, 0);

    if (appLoadingCount > 0) {
        return;
    }

    const overlay = document.getElementById("app-loading-overlay");
    if (overlay) {
        overlay.classList.remove("is-visible");
    }
    document.body.classList.remove("is-app-loading");
};

window.withAppLoading = async function (message, action) {
    showAppLoading(message);
    try {
        return await action();
    } finally {
        hideAppLoading();
    }
};

window.showConfirmDialog = function ({
    title = "Confirmar accion",
    message = "Desea continuar?",
    confirmText = "Confirmar",
    cancelText = "Cancelar",
    tone = "danger"
} = {}) {
    return new Promise(resolve => {
        const escapeHtml = value => String(value).replace(/[&<>"']/g, character => ({
            "&": "&amp;",
            "<": "&lt;",
            ">": "&gt;",
            "\"": "&quot;",
            "'": "&#039;"
        })[character]);

        const overlay = document.createElement("div");
        overlay.className = "app-confirm-overlay";
        overlay.innerHTML = `
            <section class="app-confirm-dialog ${tone === "danger" ? "danger" : ""}" role="dialog" aria-modal="true" aria-labelledby="app-confirm-title" aria-describedby="app-confirm-message">
                <div class="app-confirm-icon" aria-hidden="true">!</div>
                <div class="app-confirm-content">
                    <h2 id="app-confirm-title">${escapeHtml(title)}</h2>
                    <p id="app-confirm-message">${escapeHtml(message)}</p>
                </div>
                <div class="app-confirm-actions">
                    <button class="app-confirm-cancel" type="button">${escapeHtml(cancelText)}</button>
                    <button class="app-confirm-accept" type="button">${escapeHtml(confirmText)}</button>
                </div>
            </section>
        `;

        const close = value => {
            document.removeEventListener("keydown", onKeydown);
            overlay.classList.add("is-leaving");
            overlay.addEventListener("transitionend", () => overlay.remove(), { once: true });
            resolve(value);
        };

        const onKeydown = event => {
            if (event.key === "Escape") {
                close(false);
            }
        };

        overlay.addEventListener("click", event => {
            if (event.target === overlay) {
                close(false);
            }
        });

        overlay.querySelector(".app-confirm-cancel").addEventListener("click", () => close(false));
        overlay.querySelector(".app-confirm-accept").addEventListener("click", () => close(true));

        document.body.appendChild(overlay);
        document.addEventListener("keydown", onKeydown);
        overlay.querySelector(".app-confirm-accept").focus();
    });
};

window.showMessageDialog = function ({
    title = "Mensaje",
    message = "",
    confirmText = "Aceptar"
} = {}) {
    return new Promise(resolve => {
        const escapeHtml = value => String(value).replace(/[&<>"']/g, character => ({
            "&": "&amp;",
            "<": "&lt;",
            ">": "&gt;",
            "\"": "&quot;",
            "'": "&#039;"
        })[character]);

        const overlay = document.createElement("div");
        overlay.className = "app-confirm-overlay";
        overlay.innerHTML = `
            <section class="app-confirm-dialog info" role="dialog" aria-modal="true" aria-labelledby="app-message-title" aria-describedby="app-message-text">
                <div class="app-confirm-icon" aria-hidden="true">i</div>
                <div class="app-confirm-content">
                    <h2 id="app-message-title">${escapeHtml(title)}</h2>
                    <p id="app-message-text">${escapeHtml(message)}</p>
                </div>
                <div class="app-confirm-actions">
                    <button class="app-confirm-accept" type="button">${escapeHtml(confirmText)}</button>
                </div>
            </section>
        `;

        const close = () => {
            overlay.classList.add("is-leaving");
            overlay.addEventListener("transitionend", () => overlay.remove(), { once: true });
            resolve(true);
        };

        overlay.querySelector(".app-confirm-accept").addEventListener("click", close);

        document.body.appendChild(overlay);
        overlay.querySelector(".app-confirm-accept").focus();
    });
};

(function () {
    async function acceptMessage(clave) {
        await fetch(`/api/mensajes/${encodeURIComponent(clave)}/aceptar`, {
            method: "POST"
        });
    }

    async function showPendingMessages() {
        if (window.location.pathname === "/") {
            return;
        }

        let response;
        try {
            response = await fetch("/api/mensajes/pendientes");
        } catch {
            return;
        }

        if (!response.ok) {
            return;
        }

        const messages = await response.json();
        for (const message of messages) {
            await window.showMessageDialog({
                title: message.titulo || "Mensaje",
                message: message.mensaje || "",
                confirmText: "Aceptar"
            });
            await acceptMessage(message.clave);
        }
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", showPendingMessages, { once: true });
    } else {
        showPendingMessages();
    }
})();
