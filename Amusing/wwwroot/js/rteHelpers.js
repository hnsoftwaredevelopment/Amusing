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

    function _insertTextIntoRteInstance(rteInstance, text) {
        // Insert using instance.executeCommand if available
        try {
            if (rteInstance && typeof rteInstance.executeCommand === 'function') {
                // Some Syncfusion versions expose executeCommand
                rteInstance.executeCommand('insertText', text);
                return true;
            }

            // If there's an editorManager with execCommand (classic approach)
            if (rteInstance && rteInstance.editorManager && typeof rteInstance.editorManager.execCommand === 'function') {
                rteInstance.editorManager.execCommand('insertText', text);
                return true;
            }

            // Some versions expose getDocument/selection and rely on document.execCommand fallback
            // Best-effort fallback: try to use the DOM selection/execCommand
            try {
                if (document.queryCommandSupported && document.queryCommandSupported('insertText')) {
                    document.execCommand('insertText', false, text);
                    return true;
                }
            } catch (e) {
                // ignore fallback failure
            }

        } catch (e) {
            // swallow internal errors from unknown syncfusion internals
            console.debug('rteHelpers._insertTextIntoRteInstance error', e);
        }
        return false;
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
    window.rteHelpers.insertTextAtCursor = function (rteIdOrInstance, text) {
        try {
            var instance = null;

            // If caller passed the instance itself
            if (rteIdOrInstance && typeof rteIdOrInstance === 'object' && (rteIdOrInstance.ej2_instances || rteIdOrInstance.element || rteIdOrInstance.editorManager)) {
                // likely already an element or instance — normalize
                if (rteIdOrInstance.ej2_instances) {
                    instance = rteIdOrInstance.ej2_instances[0];
                } else {
                    instance = rteIdOrInstance;
                }
            } else if (typeof rteIdOrInstance === 'string') {
                // treat as element id
                var el = document.getElementById(rteIdOrInstance);
                instance = el?.ej2_instances?.[0] || null;
            }

            if (!instance) {
                // nothing to insert into
                return false;
            }

            return _insertTextIntoRteInstance(instance, text);
        } catch (e) {
            console.debug('rteHelpers.insertTextAtCursor error', e);
            return false;
        }
    };
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
    
    window.rteHelpers.updateContextMenu = function (rteId, items) {
        try {
            var rteEl = document.getElementById(rteId);
            if (!rteEl) {
                console.warn('rteHelpers.updateContextMenu: rte element not found', rteId);
                return false;
            }

            // try get the instance in multiple ways (robust)
            var instance = rteEl?.ej2_instances?.[0] || null;
            if (!instance) {
                // scan children quickly for an instance
                var children = rteEl.querySelectorAll('*');
                for (var i = 0; i < children.length && !instance; i++) {
                    var c = children[i];
                    if (c && c.ej2_instances && c.ej2_instances[0]) instance = c.ej2_instances[0];
                }
            }

            // Remove previously attached custom menu handler to avoid duplicates
            // We store a flag on the element so we can know if we attached already.
            if (rteEl._rteHelpersContextHandlerAttached) {
                // update items only (menu recreated on open), still keep single handler
                rteEl._rteHelpersContextItems = (Array.isArray(items) ? items.slice() : []);
                return true;
            }

            // store items to be used by the handler
            rteEl._rteHelpersContextItems = (Array.isArray(items) ? items.slice() : []);

            // Add the listener on the element (not on instance) so it survives instance internals
            rteEl.addEventListener('contextmenu', function (ev) {
                ev.preventDefault();

                // get latest items (they may have been replaced)
                var menuItems = rteEl._rteHelpersContextItems || [];
                if (!menuItems || menuItems.length === 0) {
                    return;
                }

                // remove existing custom menu if any
                var existing = document.getElementById('rteCustomContextMenu');
                if (existing) existing.remove();

                // create menu DOM
                var menu = document.createElement('ul');
                menu.id = 'rteCustomContextMenu';
                menu.style.position = 'absolute';
                menu.style.top = (ev.pageY) + 'px';
                menu.style.left = (ev.pageX) + 'px';
                menu.style.background = '#fff';
                menu.style.border = '1px solid rgba(0,0,0,0.12)';
                menu.style.padding = '4px';
                menu.style.zIndex = 2147483647; // very high
                menu.style.listStyle = 'none';
                menu.style.borderRadius = '4px';
                menu.style.boxShadow = '0 2px 10px rgba(0,0,0,0.12)';
                menu.style.minWidth = '160px';

                // append items
                menuItems.forEach(function (it) {
                    var li = document.createElement('li');
                    li.textContent = it.text || '';
                    li.style.padding = '6px 10px';
                    li.style.cursor = 'pointer';
                    li.style.userSelect = 'none';
                    li.onmouseenter = function () { li.style.background = 'rgba(0,0,0,0.04)'; };
                    li.onmouseleave = function () { li.style.background = 'transparent'; };
                    li.onclick = function (clickEv) {
                        // prevent closing race
                        clickEv.preventDefault();

                        // determine instance on click (re-evaluate — instance might exist on a child element)
                        var inst = instance;
                        if (!inst) {
                            inst = rteEl?.ej2_instances?.[0] || null;
                            if (!inst) {
                                // scan children quick
                                var ch = rteEl.querySelectorAll('*');
                                for (var k = 0; k < ch.length && !inst; k++) {
                                    var cc = ch[k];
                                    if (cc && cc.ej2_instances && cc.ej2_instances[0]) inst = cc.ej2_instances[0];
                                }
                            }
                        }

                        // try to insert using the best available API
                        var inserted = false;
                        if (inst) {
                            inserted = _insertTextIntoRteInstance(inst, it.text);
                        }

                        // fallback: try document.execCommand
                        if (!inserted) {
                            try { document.execCommand('insertText', false, it.text); } catch (e) { /* ignore */ }
                        }

                        // remove menu
                        menu.remove();
                    };
                    menu.appendChild(li);
                });

                document.body.appendChild(menu);

                // remove when clicking elsewhere
                var rmHandler = function () {
                    var el = document.getElementById('rteCustomContextMenu');
                    if (el) el.remove();
                    document.removeEventListener('click', rmHandler);
                };
                setTimeout(function () { document.addEventListener('click', rmHandler); }, 0);
            });

            rteEl._rteHelpersContextHandlerAttached = true;
            return true;
        } catch (err) {
            console.debug('rteHelpers.updateContextMenu error', err);
            return false;
        }
    };
    window.rteHelpers.isRteReady = function (elementId) {
        const inst = _findRteInstance(elementId);
        return !!inst?.element;
    };

})();