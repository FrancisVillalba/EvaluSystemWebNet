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
