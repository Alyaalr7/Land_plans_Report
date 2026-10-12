// جدول العمليات (ملف المستخدم + سجل التدقيق): علامات التصنيف + البحث + ترقيم تلقائي
document.querySelectorAll('[data-activity]').forEach(function (box) {
    var chips = box.querySelectorAll('[data-chip]');
    var search = box.querySelector('[data-activity-search]');
    var rows = box.querySelectorAll('tr[data-cat]');
    var empty = box.querySelector('.dga-table-empty-row');
    var cat = '';

    function apply() {
        var text = search ? search.value.trim().toLowerCase() : '';
        var n = 0;
        rows.forEach(function (r) {
            var ok = (!cat || r.dataset.cat === cat)
                && (!text || r.dataset.search.toLowerCase().indexOf(text) !== -1);
            r.hidden = !ok;
            if (ok) {
                n++;
                var num = r.querySelector('.row-num');
                if (num) num.textContent = n;
            }
        });
        if (empty) empty.hidden = n > 0;
    }

    chips.forEach(function (c) {
        c.addEventListener('click', function () {
            cat = c.dataset.chip;
            chips.forEach(function (x) { x.setAttribute('aria-pressed', x === c ? 'true' : 'false'); });
            apply();
        });
    });
    if (search) search.addEventListener('input', apply);
});
