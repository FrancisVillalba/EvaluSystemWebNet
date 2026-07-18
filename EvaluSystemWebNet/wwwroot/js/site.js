window.showToast = function (message, type = "success") {
    const host = document.getElementById("toast-host");

    if (!host) {
        return;
    }

    const toast = document.createElement("div");
    toast.className = `app-toast ${type === "error" ? "error" : "success"}`;
    toast.setAttribute("role", type === "error" ? "alert" : "status");

    const icon = type === "error" ? "!" : "âœ“";
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
    const appBasePath = (document.querySelector('meta[name="app-base-path"]')?.content || "/" ).replace(/\/$/, "" );
    const apiRoot = `${appBasePath}/api/`.toLowerCase();
    let redirectingToLogin = false;

    function withAppBasePath(input) {
        if (typeof input === "string" && input.startsWith("/api/")) {
            return `${appBasePath}${input}`;
        }

        return input;
    }

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
        return path.startsWith(apiRoot) && !path.startsWith(`${apiRoot}auth/`);
    }

    function redirectToLogin() {
        redirectingToLogin = true;

        if (window.location.pathname !== `${appBasePath}/`) {
            window.showToast?.("La sesion caduco. Inicie sesion nuevamente.", "error");
        }

        window.setTimeout(() => {
            const target = window.top || window;
            target.location.href = document.querySelector('meta[name="app-base-path"]')?.content || "/";
        }, 600);
    }

    window.fetch = async function (input, init) {
        const resolvedInput = withAppBasePath(input);
        const response = await nativeFetch(resolvedInput, init);

        if (shouldRedirectToLogin(resolvedInput, response)) {
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
    function closeUserMenus(exceptMenu) {
        document.querySelectorAll("[data-user-menu]").forEach(menu => {
            if (menu === exceptMenu) {
                return;
            }

            const toggle = menu.querySelector("[data-user-menu-toggle]");
            const dropdown = menu.querySelector("[data-user-menu-dropdown]");
            toggle?.setAttribute("aria-expanded", "false");
            if (dropdown) {
                dropdown.hidden = true;
            }
        });
    }

    function setPasswordError(message) {
        const error = document.querySelector("[data-password-error]");
        if (!error) {
            return;
        }

        error.textContent = message || "";
        error.hidden = !message;
    }

    function openPasswordModal() {
        const modal = document.querySelector("[data-password-modal]");
        const form = document.querySelector("[data-password-form]");
        if (!modal || !form) {
            return;
        }

        form.reset();
        setPasswordError("");
        modal.hidden = false;
        document.getElementById("current-password")?.focus();
    }

    function closePasswordModal() {
        const modal = document.querySelector("[data-password-modal]");
        if (modal) {
            modal.hidden = true;
        }
        setPasswordError("");
    }

    async function logout(event) {
        event?.preventDefault();

        try {
            await fetch("/api/auth/logout", { method: "POST" });
        } finally {
            const target = window.top || window;
            target.location.href = document.querySelector('meta[name="app-base-path"]')?.content || "/";
        }
    }

    async function submitPasswordChange(event) {
        event.preventDefault();

        const form = event.currentTarget;
        const submitButton = form.querySelector(".app-password-submit");
        const formData = new FormData(form);
        const payload = {
            currentPassword: String(formData.get("currentPassword") || ""),
            newPassword: String(formData.get("newPassword") || ""),
            confirmPassword: String(formData.get("confirmPassword") || "")
        };

        if (!payload.currentPassword || !payload.newPassword || !payload.confirmPassword) {
            setPasswordError("Complete todos los campos.");
            return;
        }

        if (payload.newPassword.length < 6) {
            setPasswordError("La nueva contrasena debe tener al menos 6 caracteres.");
            return;
        }

        if (payload.newPassword !== payload.confirmPassword) {
            setPasswordError("La confirmacion no coincide con la nueva contrasena.");
            return;
        }

        submitButton.disabled = true;
        setPasswordError("");

        try {
            const response = await fetch("/api/auth/change-password", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(payload)
            });

            if (!response.ok) {
                let message = "No se pudo cambiar la contrasena.";
                try {
                    const error = await response.json();
                    message = error.message || message;
                } catch {
                    // Keep the generic message if the API did not return JSON.
                }

                setPasswordError(message);
                return;
            }

            closePasswordModal();
            window.showToast?.("Contrasena actualizada correctamente.");
        } catch {
            setPasswordError("No se pudo conectar con el servidor.");
        } finally {
            submitButton.disabled = false;
        }
    }

    function setupUserMenu() {
        document.querySelectorAll("[data-user-menu-toggle]").forEach(toggle => {
            toggle.addEventListener("click", event => {
                const menu = event.currentTarget.closest("[data-user-menu]");
                const dropdown = menu?.querySelector("[data-user-menu-dropdown]");
                if (!menu || !dropdown) {
                    return;
                }

                const isOpen = dropdown.hidden;
                closeUserMenus(menu);
                dropdown.hidden = !isOpen;
                toggle.setAttribute("aria-expanded", String(isOpen));
            });
        });

        document.querySelectorAll("[data-logout]").forEach(button => {
            button.addEventListener("click", logout);
        });

        document.querySelectorAll("[data-open-change-password]").forEach(button => {
            button.addEventListener("click", () => {
                closeUserMenus();
                openPasswordModal();
            });
        });

        document.querySelectorAll("[data-password-close]").forEach(button => {
            button.addEventListener("click", closePasswordModal);
        });

        document.querySelector("[data-password-form]")?.addEventListener("submit", submitPasswordChange);

        document.addEventListener("click", event => {
            if (!event.target.closest("[data-user-menu]")) {
                closeUserMenus();
            }
        });

        document.addEventListener("keydown", event => {
            if (event.key === "Escape") {
                closeUserMenus();
                closePasswordModal();
            }
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", setupUserMenu, { once: true });
    } else {
        setupUserMenu();
    }
})();

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
