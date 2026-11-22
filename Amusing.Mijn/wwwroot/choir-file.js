(() => {
    const profileStates = new WeakMap();
    const previewUrls = new WeakMap();
    let submitting = false;
    let savingImages = false;

    function releasePreviewUrl(preview) {
        const url = previewUrls.get(preview);
        if (url) URL.revokeObjectURL(url);
        previewUrls.delete(preview);
    }

    function values(form) {
        return JSON.stringify([...new FormData(form)].filter(([key]) => key.startsWith("Input.")));
    }

    function initialize() {
        for (const form of document.querySelectorAll("form[data-choir-profile]")) {
            if (!profileStates.has(form)) profileStates.set(form, values(form));
        }
    }

    function updateProfile(form) {
        const before = profileStates.get(form);
        const dirty = before !== undefined && before !== values(form);
        form.toggleAttribute("data-unsaved", dirty);
        form.querySelector("[data-profile-save-bar]").classList.toggle("is-dirty", dirty);
        form.querySelector("[data-profile-warning]").hidden = !dirty;
    }

    document.addEventListener("input", event => {
        const form = event.target.closest("form[data-choir-profile]");
        if (form) updateProfile(form);
    });

    document.addEventListener("change", event => {
        const input = event.target;
        const profile = input.closest("form[data-choir-profile]");
        if (profile) updateProfile(profile);
        if (!(input instanceof HTMLInputElement) || !input.matches("[data-choir-file]")) return;

        const form = input.form;
        const file = input.files[0];
        const name = form.querySelector("[data-file-name]");
        name.dataset.placeholder ??= name.textContent.trim();
        name.textContent = file?.name ?? name.dataset.placeholder;
        form.toggleAttribute("data-unsaved", Boolean(file));
        form.querySelector("[data-image-error]").hidden = true;
        updateImageBar();
        const preview = form.querySelector("[data-image-preview]");
        if (!(preview instanceof HTMLImageElement)) return;
        releasePreviewUrl(preview);
        preview.removeAttribute("src");
        preview.hidden = true;

        if (file && /^image\/(png|jpeg|webp)$/.test(file.type)) {
            const url = URL.createObjectURL(file);
            previewUrls.set(preview, url);
            preview.src = url;
            preview.hidden = false;
        }
    });

    function dropZone(event) {
        return event.target.closest("[data-image-drop]");
    }

    document.addEventListener("dragover", event => {
        const zone = dropZone(event);
        if (!zone || !event.dataTransfer.types.includes("Files")) return;
        event.preventDefault();
        event.dataTransfer.dropEffect = "copy";
        zone.classList.add("is-dragging");
    });

    document.addEventListener("dragleave", event => {
        const zone = dropZone(event);
        if (zone && !zone.contains(event.relatedTarget)) zone.classList.remove("is-dragging");
    });

    document.addEventListener("drop", event => {
        const zone = dropZone(event);
        if (!zone || !event.dataTransfer.types.includes("Files")) return;
        event.preventDefault();
        zone.classList.remove("is-dragging");
        const file = event.dataTransfer.files[0];
        if (!file) return;
        const input = zone.querySelector("[data-choir-file]");
        const files = new DataTransfer();
        files.items.add(file);
        input.files = files.files;
        input.dispatchEvent(new Event("change", { bubbles: true }));
    });

    function updateImageBar() {
        const bar = document.querySelector("[data-image-save-bar]");
        if (bar) bar.hidden = !document.querySelector("form[data-image-upload][data-unsaved]");
    }

    async function saveImages() {
        if (savingImages) return;
        const forms = [...document.querySelectorAll("form[data-image-upload][data-unsaved]")];
        if (!forms.length) return;

        savingImages = true;
        const bar = document.querySelector("[data-image-save-bar]");
        const status = bar.querySelector("[data-image-save-status]");
        const controls = [...document.querySelectorAll("#afbeeldingen button, [data-choir-file]")];
        const uploads = forms.map(form => ({ form, data: new FormData(form) }));
        controls.forEach(control => control.disabled = true);
        const unsavedText = status.textContent;
        status.textContent = bar.dataset.saving;
        let savedUrl;
        try {
            for (const { form, data } of uploads) {
                const error = form.querySelector("[data-image-error]");
                error.hidden = true;
                try {
                    const response = await fetch(form.action, { method: "POST", body: data });
                    const url = new URL(response.url);
                    if (!response.ok || !response.redirected || url.searchParams.get("afbeelding") !== "saved") {
                        const code = url.searchParams.get("afbeelding");
                        const detail = bar.dataset.serverDetail === "1" ? url.searchParams.get("uploadmelding") : null;
                        error.textContent = detail || { conflict: bar.dataset.conflict, size: bar.dataset.size, busy: bar.dataset.busy }[code] || bar.dataset.error;
                        error.hidden = false;
                        continue;
                    }
                    savedUrl = url.href;
                    const card = form.closest("[data-image-card]");
                    const page = new DOMParser().parseFromString(await response.text(), "text/html");
                    const replacement = [...page.querySelectorAll("[data-image-card]")]
                        .find(item => item.dataset.imageCard === card.dataset.imageCard);
                    form.removeAttribute("data-unsaved");
                    const preview = form.querySelector("[data-image-preview]");
                    releasePreviewUrl(preview);
                    if (replacement) card.replaceWith(replacement);
                    else {
                        const input = form.querySelector("[data-choir-file]");
                        input.value = "";
                        input.dispatchEvent(new Event("change", { bubbles: true }));
                    }
                } catch {
                    error.textContent = bar.dataset.error;
                    error.hidden = false;
                }
            }
        } finally {
            savingImages = false;
            controls.forEach(control => control.disabled = false);
            status.textContent = unsavedText;
            updateImageBar();
        }

        if (savedUrl && bar.hidden) window.location.assign(savedUrl);
    }

    document.addEventListener("submit", event => {
        const form = event.target;
        if (form.matches?.("[data-image-save-bar], [data-image-upload]")) {
            event.preventDefault();
            void saveImages();
            return;
        }
        if (!form.matches?.("form[data-choir-profile], #afbeeldingen form")) return;
        const other = [...document.querySelectorAll("form[data-unsaved]")].some(candidate => candidate !== form);
        if (other && !window.confirm(document.querySelector("#afbeeldingen").dataset.discardMessage)) {
            event.preventDefault();
            return;
        }
        submitting = true;
    });

    window.addEventListener("beforeunload", event => {
        if (submitting || !document.querySelector("form[data-unsaved]")) return;
        event.preventDefault();
        event.returnValue = true;
    });
    window.addEventListener("pageshow", () => { submitting = false; initialize(); });
    initialize();
    window.Blazor?.addEventListener("enhancedload", initialize);
})();
