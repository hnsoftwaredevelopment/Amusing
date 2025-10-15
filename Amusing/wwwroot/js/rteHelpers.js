(function () {
    var caretPositions = {};

    function _findInputById(id) {
        var el = document.getElementById(id) || document.querySelector('#' + id);
        if (!el) {
            el = document.querySelector('input.e-input'); // fallback Syncfusion
        }
        if (el && el.tagName !== 'INPUT') {
            var found = el.querySelector && el.querySelector('input');
            if (found) el = found;
        }
        return el || null;
    }

    function registerInput(id) {
        var el = _findInputById(id);
        if (!el) return false;
        if (el._rteHelpersRegistered) return true;

        function update() {
            caretPositions[id] = el.selectionStart || 0;
        }

        el.addEventListener('keyup', update);
        el.addEventListener('click', update);
        el.addEventListener('input', update);
        el._rteHelpersRegistered = true;
        caretPositions[id] = el.selectionStart || 0;
        return true;
    }

    function getLastCaret(id) {
        return caretPositions[id] || 0;
    }

    function getActiveValue() {
        var el = document.activeElement;
        return (el && el.value) ? el.value : "";
    }

    function setCaretById(id, pos) {
        var el = _findInputById(id);
        if (!el) return false;
        try {
            el.focus();
            if (typeof el.setSelectionRange === "function") el.setSelectionRange(pos, pos);
            return true;
        } catch (e) {
            return false;
        }
    }

    function _findRteInstance(elementId) {
        try {
            var el = document.getElementById(elementId) || document.querySelector('#' + elementId);
            if (el && el.ej2_instances && el.ej2_instances[0]) return el.ej2_instances[0];

            if (el) {
                var children = el.getElementsByTagName("*");
                for (var i = 0; i < children.length; i++) {
                    var c = children[i];
                    if (c && c.ej2_instances && c.ej2_instances[0]) return c.ej2_instances[0];
                }
            }

            // fallback: global scan (expensive)
            var all = document.getElementsByTagName("*");
            for (var j = 0; j < all.length; j++) {
                var a = all[j];
                if (a && a.ej2_instances && a.ej2_instances[0]) {
                    var inst = a.ej2_instances[0];
                    try {
                        if (inst.element && inst.element.id && inst.element.id.indexOf(elementId) !== -1) return inst;
                        if (a.classList && a.classList.contains('e-richtexteditor')) return inst;
                    } catch (_) { /* ignore */ }
                }
            }
        } catch (e) {
            console.debug('rteHelpers._findRteInstance error', e);
        }
        return null;
    }

    function _applyMenuUpdate(inst, normalized) {
        let updated = false;

        const safe = (fn) => {
            try {
                fn();
                return true;
            } catch (e) {
                console.debug('rteHelpers.safe ignored error:', e);
                return false;
            }
        };

        // 1. Direct slashMenuSettings update
        safe(() => {
            inst.slashMenuSettings = inst.slashMenuSettings || {};
            inst.slashMenuSettings.items = normalized;
            updated = true;
        });

        // 2. Module updates
        safe(() => {
            if (!inst.slashMenuModule) return;
            const mod = inst.slashMenuModule;

            // Module zelf
            if ('items' in mod) {
                mod.items = normalized;
                updated = true;
            }

            // Subcomponent (menuObj of listObj)
            const menuObj = mod.menuObj || mod.listObj;
            if (menuObj) {
                safe(() => { if ('fields' in menuObj) menuObj.fields = { text: 'text' }; });
                safe(() => {
                    if ('dataSource' in menuObj) {
                        menuObj.dataSource = normalized.map(x => ({ text: x.text, iconCss: x.iconCss }));
                        if (typeof menuObj.dataBind === 'function') menuObj.dataBind();
                        updated = true;
                    }
                });
            }
        });

        // 3. Als er een losstaande menuObj bestaat op de instance
        safe(() => {
            if (!inst.menuObj) return;
            const menuObj = inst.menuObj;

            safe(() => { if ('fields' in menuObj) menuObj.fields = { text: 'text' }; });
            safe(() => {
                if ('dataSource' in menuObj) {
                    menuObj.dataSource = normalized.map(x => ({ text: x.text, iconCss: x.iconCss }));
                    if (typeof menuObj.dataBind === 'function') menuObj.dataBind();
                    updated = true;
                }
            });
        });

        // 4. DOM fallback – brute force rebuild
        safe(() => {
            const m = inst.element?.querySelector?.('.e-rte-slash-menu, .e-list-parent');
            if (m) {
                m.innerHTML = '';
                normalized.forEach(it => {
                    const li = document.createElement('div');
                    li.className = 'e-list-item';
                    li.textContent = it.text || '';
                    m.appendChild(li);
                });
                updated = true;
            }
        });

        // 5. Herbind of ververs de component
        safe(() => {
            if (typeof inst.dataBind === 'function') inst.dataBind();
            else if (typeof inst.refresh === 'function') inst.refresh();
        });

        return updated;
    }

    function updateSlashMenu(elementId, items) {
        try {
            var normalized = Array.isArray(items)
                ? items.map(i => ({ text: i?.text || '', iconCss: i?.iconCss || '' }))
                : [];

            // Kleine vertraging, maar probeer meerdere keren (RTE kan traag laden)
            let attempts = 0;
            const maxAttempts = 10;

            const tryUpdate = () => {
                const inst = _findRteInstance(elementId);
                if (!inst || !inst.element) {
                    if (attempts++ < maxAttempts) {
                        setTimeout(tryUpdate, 100); // probeer opnieuw na 100ms
                    } else {
                        console.warn(`rteHelpers.updateSlashMenu: geen instance gevonden voor ${elementId}`);
                    }
                    return;
                }
                const applied = _applyMenuUpdate(inst, normalized);
                console.debug('rteHelpers.updateSlashMenu: applied?', applied, 'count', normalized.length, 'elementId', elementId);
            };

            setTimeout(tryUpdate, 50);
            return true;
        } catch (e) {
            console.error('rteHelpers.updateSlashMenu error:', e?.message || e);
            return false;
        }
    }

    window.rteHelpers = window.rteHelpers || {};
    window.rteHelpers.isRteReady = function (elementId) {
        try {
            var el = document.getElementById(elementId) || document.querySelector('#' + elementId);
            if (!el) return false;

            // vaak zit de client instance in ej2_instances op het element of een child
            var inst = (el.ej2_instances && el.ej2_instances[0]) || null;
            if (!inst) {
                var children = el.querySelectorAll('*');
                for (var i = 0; i < children.length; i++) {
                    var c = children[i];
                    if (c && c.ej2_instances && c.ej2_instances[0]) {
                        inst = c.ej2_instances[0];
                        break;
                    }
                }
            }

            // als we een instance hebben en die een element property heeft, is het meestal OK
            return inst && inst.element ? true : false;
        } catch (e) {
            // geen paniek — return false zodat C# kan retryen
            console.debug('rteHelpers.isRteReady error', e);
            return false;
        }
    };

    window.rteHelpers.registerInput = registerInput;
    window.rteHelpers.getLastCaret = getLastCaret;
    window.rteHelpers.getActiveValue = getActiveValue;
    window.rteHelpers.setCaretById = setCaretById;
    window.rteHelpers.updateSlashMenu = updateSlashMenu;
})();