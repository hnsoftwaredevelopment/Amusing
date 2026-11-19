document.addEventListener("click", event => {
    const button = event.target.closest?.("[data-password-toggle]");
    if (!button) return;

    const input = document.getElementById(button.getAttribute("aria-controls"));
    if (!(input instanceof HTMLInputElement)) return;

    const show = input.type === "password";
    input.type = show ? "text" : "password";
    button.setAttribute("aria-pressed", String(show));
    button.setAttribute("aria-label", show ? button.dataset.hideLabel : button.dataset.showLabel);
});
