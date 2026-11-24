(() => {
    function filterGrid() {
        const input = document.querySelector('[data-support-search]');
        if (!input) return;
        const term = input.value.trim().toLocaleLowerCase();
        let count = 0;
        document.querySelectorAll('[data-support-row]').forEach(row => {
            row.hidden = !row.dataset.searchName.toLocaleLowerCase().includes(term);
            if (!row.hidden) count++;
        });
        const status = document.querySelector('[data-support-count]');
        status.textContent = status.dataset.countFormat.replace('{0}', String(count));
        document.querySelector('[data-support-empty]').hidden = count !== 0;
    }
    document.addEventListener('input', event => {
        if (event.target.matches('[data-support-search]')) filterGrid();
    });
    document.addEventListener('submit', event => {
        if (event.target.matches('form[role="search"]') && event.target.querySelector('[data-support-search]')) {
            event.preventDefault();
            filterGrid();
        }
    });
    window.addEventListener('pageshow', filterGrid);
    window.Blazor?.addEventListener('enhancedload', filterGrid);
    filterGrid();
})();
