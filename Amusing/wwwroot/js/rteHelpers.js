(function () {
    window.rteHelpers = window.rteHelpers || {};

    // --- Internal caret storage ---
    const caretPositions = {};
    window.rteHelpers.caretPositions = caretPositions;

    // --- Helpers ---
    function _findRteInstance(elementId) {
        const el = document.getElementById(elementId);
        if (el?.ej2_instances?.[0]) return el.ej2_instances[0];

        // scan children quickly
        if (el) {
            const children = el.getElementsByTagName("*");
            for (let i = 0; i < children.length; i++) {
                const c = children[i];
                if (c?.ej2_instances?.[0]) return c.ej2_instances[0];
            }
        }

        // global scan fallback
        const all = document.getElementsByTagName("*");
        for (let j = 0; j < all.length; j++) {
            const a = all[j];
            if (a?.ej2_instances?.[0]) return a.ej2_instances[0];
        }
        return null;
    }

    function _insertTextIntoRteInstance(rteInstance, text) {
        try {
            if (!rteInstance) return false;
            if (typeof rteInstance.executeCommand === 'function') {
                rteInstance.executeCommand('insertText', text);
                return true;
            }
            if (rteInstance.editorManager?.execCommand) {
                rteInstance.editorManager.execCommand('insertText', text);
                return true;
            }
            document.execCommand('insertText', false, text);
            return true;
        } catch (e) {
            console.debug('rteHelpers._insertTextIntoRteInstance error', e);
            return false;
        }
    }

    function getRteInstance(rteIdOrInstance) {
        if (!rteIdOrInstance) return null;
        return typeof rteIdOrInstance === 'string'
            ? document.getElementById(rteIdOrInstance)?.ej2_instances?.[0]
            : rteIdOrInstance;
    }

    // --- Public functions ---
    window.rteHelpers.insertTextAtCursor = function (rteIdOrInstance, text) {
        const rte = getRteInstance(rteIdOrInstance);
        if (!rte) return false;
        try { rte.focusIn(); } catch { }
        return _insertTextIntoRteInstance(rte, text);
    };

    window.rteHelpers.updateContextMenu = function (rteId, items) {
        try {
            const rteEl = document.getElementById(rteId);
            if (!rteEl) return false;

            rteEl._rteHelpersContextItems = Array.isArray(items) ? items.slice() : [];

            if (rteEl._rteHelpersContextHandlerAttached) return true;

            rteEl.addEventListener('contextmenu', function (ev) {
                ev.preventDefault();
                const menuItems = rteEl._rteHelpersContextItems || [];
                if (!menuItems.length) return;

                const existing = document.getElementById('rteCustomContextMenu');
                if (existing) existing.remove();

                const menu = document.createElement('ul');
                menu.id = 'rteCustomContextMenu';
                Object.assign(menu.style, {
                    position: 'absolute',
                    top: ev.pageY + 'px',
                    left: ev.pageX + 'px',
                    background: '#fff',
                    border: '1px solid rgba(0,0,0,0.12)',
                    padding: '4px',
                    zIndex: 2147483647,
                    listStyle: 'none',
                    borderRadius: '4px',
                    boxShadow: '0 2px 10px rgba(0,0,0,0.12)',
                    minWidth: '160px'
                });

                menuItems.forEach(function (it) {
                    const li = document.createElement('li');
                    li.textContent = it.text || '';
                    li.style.padding = '6px 10px';
                    li.style.cursor = 'pointer';
                    li.style.userSelect = 'none';
                    li.onmouseenter = () => li.style.background = 'rgba(0,0,0,0.04)';
                    li.onmouseleave = () => li.style.background = 'transparent';
                    li.onclick = function (clickEv) {
                        clickEv.preventDefault();
                        const inst = getRteInstance(rteEl);
                        _insertTextIntoRteInstance(inst, it.text);
                        menu.remove();
                    };
                    menu.appendChild(li);
                });

                document.body.appendChild(menu);

                const rmHandler = () => {
                    const el = document.getElementById('rteCustomContextMenu');
                    if (el) el.remove();
                    document.removeEventListener('click', rmHandler);
                };
                setTimeout(() => document.addEventListener('click', rmHandler), 0);
            });

            rteEl._rteHelpersContextHandlerAttached = true;
            return true;
        } catch (err) {
            console.debug('rteHelpers.updateContextMenu error', err);
            return false;
        }
    };

})();
