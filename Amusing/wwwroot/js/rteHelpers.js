window.rteHelpers = (function () {
    var lastCaret = 0;

    function _findInputById(id) {
        var el = document.getElementById(id);
        if (!el) el = document.querySelector('#' + id);
        if (el && el.tagName !== 'INPUT') {
            var found = el.querySelector('input');
            if (found) el = found;
        }
        if (!el) el = document.querySelector('input.e-input'); // fallback Syncfusion
        return el || null;
    }

    function registerInput(id) {
        var el = _findInputById(id);
        if (!el) return false;
        if (el._rteHelpersRegistered) return true;

        function update() { lastCaret = el.selectionStart || 0; }

        el.addEventListener('keyup', update);
        el.addEventListener('click', update);
        el.addEventListener('input', update);

        el._rteHelpersRegistered = true;
        lastCaret = el.selectionStart || 0;
        return true;
    }

    function getLastCaret() {
        return lastCaret || 0;
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
            if (typeof el.setSelectionRange === "function") {
                el.setSelectionRange(pos, pos);
            }
            return true;
        } catch (e) {
            return false;
        }
    }

    return {
        registerInput: registerInput,
        getLastCaret: getLastCaret,
        getActiveValue: getActiveValue,
        setCaretById: setCaretById
    };
})();
