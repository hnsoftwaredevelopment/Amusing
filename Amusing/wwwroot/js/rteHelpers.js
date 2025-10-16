(function () {
    window.rteHelpers = window.rteHelpers || {};

    // --- Internal caret storage ---
    const caretPositions = {};
    window.rteHelpers.caretPositions = caretPositions;

    // --- Helpers ---
    function _findInputById(id) {
        let el = document.getElementById(id) || document.querySelector('#' + id);
        if (!el) el = document.querySelector('input.e-input'); // fallback Syncfusion
        if (el && el.tagName !== 'INPUT') {
            const found = el.querySelector && el.querySelector('input');
            if (found) el = found;
        }
        return el || null;
    }

    function _findRteInstance(elementId) {
        try {
            const el = document.getElementById(elementId) || document.querySelector('#' + elementId);
            if (el && el.ej2_instances && el.ej2_instances[0]) return el.ej2_instances[0];

            if (el) {
                const children = el.getElementsByTagName("*");
                for (let i = 0; i < children.length; i++) {
                    const c = children[i];
                    if (c && c.ej2_instances && c.ej2_instances[0]) return c.ej2_instances[0];
                }
            }

            // Global scan fallback
            const all = document.getElementsByTagName("*");
            for (let j = 0; j < all.length; j++) {
                const a = all[j];
                if (a && a.ej2_instances && a.ej2_instances[0]) {
                    const inst = a.ej2_instances[0];
                    try {
                        if (inst.element && inst.element.id && inst.element.id.includes(elementId)) return inst;
                        if (a.classList && a.classList.contains('e-richtexteditor')) return inst;
                    } catch (_) { /* ignore */ }
                }
            }
        } catch (e) {
            console.debug('rteHelpers._findRteInstance error', e);
        }
        return null;
    }

    function _applyMenuUpdate(inst, items) {
        let updated = false;
        const safe = (fn) => { try { fn(); return true; } catch (e) { console.debug('rteHelpers.safe ignored:', e); return false; } };

        safe(() => {
            inst.slashMenuSettings = inst.slashMenuSettings || {};
            inst.slashMenuSettings.items = items;
            updated = true;
        });

        safe(() => {
            const mod = inst.slashMenuModule;
            if (!mod) return;

            if ('items' in mod) mod.items = items;

            const menuObj = mod.menuObj || mod.listObj;
            if (menuObj) {
                safe(() => { if ('fields' in menuObj) menuObj.fields = { text: 'text' }; });
                safe(() => {
                    if ('dataSource' in menuObj) {
                        menuObj.dataSource = items.map(x => ({ text: x.text, iconCss: x.iconCss }));
                        if (typeof menuObj.dataBind === 'function') menuObj.dataBind();
                        updated = true;
                    }
                });
            }
        });

        safe(() => {
            const menuObj = inst.menuObj;
            if (!menuObj) return;
            safe(() => { if ('fields' in menuObj) menuObj.fields = { text: 'text' }; });
            safe(() => {
                if ('dataSource' in menuObj) {
                    menuObj.dataSource = items.map(x => ({ text: x.text, iconCss: x.iconCss }));
                    if (typeof menuObj.dataBind === 'function') menuObj.dataBind();
                    updated = true;
                }
            });
        });

        safe(() => {
            const m = inst.element?.querySelector?.('.e-rte-slash-menu, .e-list-parent');
            if (m) {
                m.innerHTML = '';
                items.forEach(it => {
                    const li = document.createElement('div');
                    li.className = 'e-list-item';
                    li.textContent = it.text || '';
                    m.appendChild(li);
                });
                updated = true;
            }
        });

        safe(() => {
            try { inst.dataBind(); } catch { try { inst.refresh(); } catch { } }
            if (typeof inst.dataBind === 'function') inst.dataBind();
            else if (typeof inst.refresh === 'function') inst.refresh();
        });

        return updated;
    }

    function insertTextAtCursor(rte, text) {
        // Ensure editor and selection exist
        if (!rte || !rte.editorManager) return;

        const editor = rte.editorManager;
        const selection = rte.getSelection(); // get current selection
        if (!selection) return;

        editor.execCommand('insertText', text);
    }

    // --- Public functions ---
    window.rteHelpers.registerInput = function (id) {
        const el = _findInputById(id);
        if (!el) return false;

        if (el._rteHelpersRegistered) return true;

        const updateCaret = () => { caretPositions[id] = el.selectionStart || 0; };
        ['keyup', 'click', 'input', 'focus', 'blur'].forEach(ev => el.addEventListener(ev, updateCaret));

        el._rteHelpersRegistered = true;
        caretPositions[id] = el.selectionStart || 0;
        return true;
    };
    window.rteHelpers.getLastCaret = (id) => caretPositions[id] || 0;
    window.rteHelpers.setCaretById = function (id, pos) {
        const el = _findInputById(id);
        if (!el) return false;
        try { el.focus(); if (el.setSelectionRange) el.setSelectionRange(pos, pos); return true; } catch (e) { return false; }
    };
    window.rteHelpers.insertTextAtCursor = insertTextAtCursor;
    window.rteHelpers.updateSlashMenu = function (elementId, items) {
        const normalized = Array.isArray(items)
            ? items.map(i => ({ text: i?.text || '', iconCss: i?.iconCss || '' }))
            : [];
        let attempts = 0, maxAttempts = 10;

        const tryUpdate = () => {
            const inst = _findRteInstance(elementId);
            if (!inst?.element) {
                if (++attempts < maxAttempts) setTimeout(tryUpdate, 100);
                else console.warn(`rteHelpers.updateSlashMenu: geen instance gevonden voor ${elementId}`);
                return;
            }
            _applyMenuUpdate(inst, normalized);
        };

        setTimeout(tryUpdate, 50);
    };
    window.rteHelpers.addInsertFieldHandler = function (rteId, items) {
        const rteEl = document.getElementById(rteId);
        const rte = rteEl?.ej2_instances?.[0];
        if (!rte) return;

        rte.toolbarClick = function (args) {
            if (args.item?.tooltipText === "Voeg veld in") {
                const menu = document.createElement("ul");
                menu.className = "toolbar-insert-menu";

                items.forEach(i => {
                    const li = document.createElement("li");
                    li.textContent = i.text;
                    li.onclick = () => { _insertTextAtCursor(rteId, i.text); menu.remove(); };
                    menu.appendChild(li);
                });

                Object.assign(menu.style, {
                    position: "absolute",
                    top: "40px",
                    left: "10px",
                    background: "#fff",
                    border: "1px solid #ccc",
                    padding: "4px",
                    zIndex: 9999,
                    listStyle: "none",
                    boxShadow: "2px 2px 6px rgba(0,0,0,0.2)",
                    borderRadius: "6px"
                });

                rteEl.appendChild(menu);
                document.addEventListener("click", () => menu.remove(), { once: true });
            }
        };
    };
    window.rteHelpers.attachContextMenu = function (rteId, menuItems) {
        const rteEl = document.getElementById(rteId);
        if (!rteEl) return;

        rteEl.addEventListener('contextmenu', function (e) {
            e.preventDefault();

            const existing = document.getElementById('rteContextMenu');
            if (existing) existing.remove();

            const menu = document.createElement('ul');
            menu.id = 'rteContextMenu';
            menu.style.position = 'absolute';
            menu.style.top = e.pageY + 'px';
            menu.style.left = e.pageX + 'px';
            menu.style.background = '#fff';
            menu.style.border = '1px solid #ccc';
            menu.style.padding = '5px';
            menu.style.zIndex = '1000';

            menuItems.forEach(item => {
                const li = document.createElement('li');
                li.textContent = item.text;
                li.style.padding = '2px 5px';
                li.style.cursor = 'pointer';
                li.onclick = () => {
                    const rteInst = rteEl?.ej2_instances?.[0];
                    if (rteInst) rteInst.executeCommand('insertText', item.text);
                    menu.remove();
                };
                menu.appendChild(li);
            });

            document.body.appendChild(menu);
            document.addEventListener('click', function handler() { menu.remove(); document.removeEventListener('click', handler); });
        });
    };
    window.rteHelpers.updateContextMenu = function (rteId, items) {
        const rteEl = document.getElementById(rteId);
        const rte = rteEl?.ej2_instances?.[0];
        if (!rte) return;

        rte.element.addEventListener("contextmenu", function (e) {
            e.preventDefault();

            // Remove existing menu
            const existing = document.getElementById("customRteContextMenu");
            if (existing) existing.remove();

            const menu = document.createElement("ul");
            menu.id = "customRteContextMenu";
            menu.style.position = "absolute";
            menu.style.top = e.pageY + "px";
            menu.style.left = e.pageX + "px";
            menu.style.background = "#fff";
            menu.style.border = "1px solid #ccc";
            menu.style.padding = "4px";
            menu.style.zIndex = 10000;
            menu.style.listStyle = "none";

            items.forEach(i => {
                const li = document.createElement("li");
                li.textContent = i.text;
                li.style.cursor = "pointer";
                li.style.padding = "2px 6px";
                li.onclick = () => {
                    // Insert text at current cursor position
                    insertTextAtCursor(rte, i.text);
                    menu.remove();
                };
                menu.appendChild(li);
            });

            document.body.appendChild(menu);

            document.addEventListener("click", () => menu.remove(), { once: true });
        });
    };
    window.rteHelpers.isRteReady = function (elementId) {
        const inst = _findRteInstance(elementId);
        return !!inst?.element;
    };

})();