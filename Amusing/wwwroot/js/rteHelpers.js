// @ts-nocheck

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

    function _insertTextAtCursor(rteId, text) {
    try {
        // start with the element the user passed
        let rteEl = document.getElementById(rteId) || document.querySelector('#' + rteId);

        // quick helper to get instance from an element
        const getInstanceFromEl = (el) => {
            try { return el?.ej2_instances?.[0] || null; } catch { return null; }
        };

        let inst = getInstanceFromEl(rteEl);

        // If not found, search for any element that has ej2_instances and seems related to this id
        if (!inst) {
            const all = document.getElementsByTagName('*');
            for (let i = 0; i < all.length && !inst; i++) {
                const a = all[i];
                try {
                    if (a && a.ej2_instances && a.ej2_instances[0]) {
                        const candidate = a.ej2_instances[0];
                        // check several heuristics to match the requested rteId:
                        // - instance.element.id equals the rteId
                        // - instance.element.id contains the rteId (partial match)
                        // - the element 'a' contains the original element with rteId (structure where ID is on child)
                        const instElId = candidate.element?.id;
                        if (instElId === rteId || (instElId && instElId.includes(rteId))) {
                            inst = candidate;
                            rteEl = a;
                            break;
                        }
                        const providedEl = document.getElementById(rteId);
                        if (providedEl && a.contains && a.contains(providedEl)) {
                            inst = candidate;
                            rteEl = a;
                            break;
                        }
                    }
                } catch (e) {
                    // ignore DOM access errors on weird nodes
                }
            }
        }

        if (!inst) {
            // last resort: try to pick the first visible richtext instance on the page
            const list = Array.from(document.querySelectorAll('.e-richtexteditor'));
            for (let i = 0; i < list.length && !inst; i++) {
                inst = getInstanceFromEl(list[i]);
                if (inst) { rteEl = list[i]; break; }
            }
        }

        if (!inst) {
            console.debug(`rteHelpers._insertTextAtCursor: geen RTE instance gevonden voor id='${rteId}'`);
            return false;
        }

        // Ensure focus is back in the editor before inserting (contextmenu stole it)
        try {
            // Try instance.element, fallback to rteEl or content area
            if (inst.element && typeof inst.element.focus === 'function') {
                inst.element.focus();
            } else if (rteEl && typeof rteEl.focus === 'function') {
                rteEl.focus();
            } else {
                const content = (rteEl && rteEl.querySelector) ? rteEl.querySelector('.e-content, .e-rte-content, [contenteditable]') : null;
                if (content && typeof content.focus === 'function') content.focus();
            }
        } catch (e) { /* ignore focus failures */ }

        // Give focus a moment (helps with some browsers / Syncfusion timing)
        setTimeout(function () {
            try {
                if (typeof inst.executeCommand === 'function') {
                    inst.executeCommand('insertText', text);
                    return true;
                }
                if (inst.editorManager && typeof inst.editorManager.execCommand === 'function') {
                    inst.editorManager.execCommand('insertText', text);
                    return true;
                }
                try { document.execCommand('insertText', false, text); return true; } catch { return false; }
            } catch (e) {
                console.debug('rteHelpers._insertTextAtCursor insert error', e);
                return false;
            }
        }, 30);

        return true;
    } catch (outer) {
        console.debug('rteHelpers._insertTextAtCursor outer error', outer);
        return false;
    }
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
    window.rteHelpers.updateContextMenu = function (elementId, items) {
        const target = document.getElementById(elementId);
        if (!target) return false;

        target._rteHelpersContextItems = Array.isArray(items) ? items.slice() : [];
        if (target._rteHelpersContextHandlerAttached) return true;
        target._rteHelpersContextHandlerAttached = true;

        target.addEventListener('contextmenu', event => {
            const fields = target._rteHelpersContextItems;
            const input = target.matches('input,textarea') ? target : target.querySelector('input,textarea');
            const editable = event.target.closest('[contenteditable="true"]');
            if (!fields.length || (!editable && (!input || !input.contains(event.target)))) return;
            event.preventDefault();
            document.getElementById('rteCustomContextMenu')?.remove();

            const selection = document.getSelection();
            const range = editable && selection?.rangeCount && editable.contains(selection.anchorNode)
                ? selection.getRangeAt(0).cloneRange() : null;
            const start = input?.selectionStart;
            const end = input?.selectionEnd;

            const menu = document.createElement('ul');
            menu.id = 'rteCustomContextMenu';
            Object.assign(menu.style, {
                position: 'absolute', top: event.pageY + 'px', left: event.pageX + 'px',
                background: '#fff', border: '1px solid rgba(0,0,0,.12)', padding: '4px',
                zIndex: '2147483647', listStyle: 'none', borderRadius: '4px',
                boxShadow: '0 2px 10px rgba(0,0,0,.12)', minWidth: '160px'
            });

            for (const field of fields) {
                const row = document.createElement('li');
                const option = document.createElement('button');
                option.type = 'button';
                option.textContent = field.text;
                Object.assign(option.style, { display: 'block', width: '100%', textAlign: 'left', padding: '6px 10px', border: '0', background: 'transparent' });
                option.addEventListener('mousedown', click => click.preventDefault());
                option.addEventListener('click', () => {
                    menu.remove();
                    if (editable) {
                        editable.focus();
                        if (range) {
                            selection.removeAllRanges();
                            selection.addRange(range);
                        }
                        const editor = _findRteInstance(elementId);
                        if (!_insertTextIntoRteInstance(editor, field.text))
                            document.execCommand('insertText', false, field.text);
                    } else {
                        input.focus();
                        input.setRangeText(field.text, start ?? input.value.length, end ?? input.value.length, 'end');
                        input.dispatchEvent(new Event('input', { bubbles: true }));
                        input.dispatchEvent(new Event('change', { bubbles: true }));
                    }
                });
                row.appendChild(option);
                menu.appendChild(row);
            }

            document.body.appendChild(menu);
            document.addEventListener('click', () => menu.remove(), { once: true });
        });
        return true;
    };

    window.rteHelpers.isRteReady = function (elementId) {
        const inst = _findRteInstance(elementId);
        return !!inst?.element;
    };
    window.rteHelpers.getActiveValue = function () {
        var el = document.activeElement;
        return (el && el.value) ? el.value : "";
    };

    // Insert custom field for Subject SfAutoComplete
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
            console.log('trying insert', rteId, 'initialEl=', document.getElementById(rteId), 'instFound=', !!inst);
            console.log('instance object', inst);
            return _insertTextIntoRteInstance(instance, text);
        } catch (e) {
            console.debug('rteHelpers.insertTextAtCursor error', e);
            return false;
        }
    };

})();