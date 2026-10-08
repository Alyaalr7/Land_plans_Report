// منطقة رفع الملفات: الصور تتجمع مع كل اختيار (3 كحد أقصى، 10 ميجا للصورة)، و✕ يحذف فعلاً
document.addEventListener('DOMContentLoaded', function () {

    // تحويل الأرقام العربية (١٢٣) إلى إنجليزية في حقول الهوية والجوال
    document.addEventListener('input', function (e) {
        var el = e.target;
        if (!el.hasAttribute || !el.hasAttribute('data-latin-digits')) return;
        var v = el.value
            .replace(/[٠-٩]/g, function (d) { return '٠١٢٣٤٥٦٧٨٩'.indexOf(d); })
            .replace(/[۰-۹]/g, function (d) { return '۰۱۲۳۴۵۶۷۸۹'.indexOf(d); });
        if (v !== el.value) el.value = v;
    });


    document.querySelectorAll('[data-show-when]').forEach(function (box) {

        var name = box.dataset.showFor;
        function update() {
            var checked = document.querySelector('input[name="' + name + '"]:checked');
            box.style.display = (checked && checked.value === box.dataset.showWhen) ? '' : 'none';
        }
        document.querySelectorAll('input[name="' + name + '"]').forEach(function (r) {
            r.addEventListener('change', update);
        });
        update(); // يظهر البوكس عند التعديل أو رجوع الصفحة بخطأ
    });



    // رسالة النجاح: تختفي بعد 5 ثواني، وتتوقف عند وقوف الماوس عليها
    // نوافذ النظام: فتح / إغلاق
    document.querySelectorAll('[data-open-dialog]').forEach(function (b) {
        var d = b.parentElement.querySelector('dialog');
        if (d) b.addEventListener('click', function () { d.showModal(); });
    });
    document.querySelectorAll('dialog [data-dialog-close]').forEach(function (b) {
        b.addEventListener('click', function () { b.closest('dialog').close(); });
    });



    // رسالة النجاح: تظهر في الوسط وتقفل بعد 5 ثواني
    var okDialog = document.querySelector('[data-success-dialog]');
    if (okDialog && okDialog.showModal) {
        okDialog.showModal();
        setTimeout(function () { if (okDialog.open) okDialog.close(); }, 5000);
    }


    document.querySelectorAll('.file-upload-zone').forEach(function (zone) {
        var input = zone.querySelector('input[type=file]');
        var list = zone.parentElement.querySelector('.file-list');
        if (!input || !list) return;
        if (zone.dataset.uploadReady) return;   // حماية: لا يُفعَّل مرتين على نفس الحقل
        zone.dataset.uploadReady = '1';

        var MAX_FILES = 3;
        var MAX_SIZE = 10 * 1024 * 1024; // 10 ميجا
        var ALLOWED = ['jpg', 'jpeg', 'png', 'pdf'];
        var selected = new DataTransfer(); // كل الصور المختارة

        // رسالة داخل الصفحة بدل alert
        var msg = document.createElement('div');
        msg.className = 'file-upload-msg';
        msg.setAttribute('role', 'alert');
        msg.hidden = true;
        list.parentElement.insertBefore(msg, list);

        function showErrors(errors) {
            msg.innerHTML = '';
            msg.hidden = !errors.length;
            zone.classList.toggle('has-error', errors.length > 0);
            errors.forEach(function (t) {
                var p = document.createElement('p');
                p.textContent = t;
                msg.appendChild(p);
            });
        }

        // كل اختيار جديد ينضاف للي قبله، مع فحص الصيغة والحجم والعدد
        function addFiles(files) {
            var errors = [];
            Array.from(files).forEach(function (file) {
                var ext = (file.name.split('.').pop() || '').toLowerCase();
                if (ALLOWED.indexOf(ext) === -1) { errors.push('الملف "' + file.name + '" بصيغة غير مسموحة.'); return; }
                if (file.size > MAX_SIZE) { errors.push('الملف "' + file.name + '" أكبر من 10 ميجابايت.'); return; }
                if (selected.files.length >= MAX_FILES) { errors.push('الحد الأقصى ' + MAX_FILES + ' ملفات.'); return; }
                selected.items.add(file);
            });
            showErrors(errors.filter(function (t, i, a) { return a.indexOf(t) === i; }));
            input.files = selected.files;
            render();
        }

        // الضغط أو Enter / Space يفتح نافذة اختيار الملفات
        zone.addEventListener('click', function (e) {
            if (e.target !== input) input.click();
        });
        zone.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); input.click(); }
        });

        input.addEventListener('change', function () {
            var picked = Array.from(input.files);
            input.files = selected.files; // نرجّع القائمة المجمّعة قبل الإضافة
            addFiles(picked);
        });

        // السحب والإفلات
        ['dragenter', 'dragover'].forEach(function (ev) {
            zone.addEventListener(ev, function (e) { e.preventDefault(); zone.classList.add('is-dragover'); });
        });
        ['dragleave', 'drop'].forEach(function (ev) {
            zone.addEventListener(ev, function (e) { e.preventDefault(); zone.classList.remove('is-dragover'); });
        });
        zone.addEventListener('drop', function (e) {
            if (e.dataTransfer && e.dataTransfer.files.length) addFiles(e.dataTransfer.files);
        });

        function sizeText(bytes) {
            return bytes >= 1024 * 1024
                ? (bytes / (1024 * 1024)).toFixed(1) + ' ميجابايت'
                : Math.max(1, Math.round(bytes / 1024)) + ' كيلوبايت';
        }

        // يرسم القائمة من الصور الموجودة فعلاً في الحقل
        function render() {
            list.innerHTML = '';
            Array.from(selected.files).forEach(function (file, index) {
                var chip = document.createElement('div');
                chip.className = 'file-chip';

                var info = document.createElement('span');
                info.className = 'file-name';
                info.innerHTML = '<svg class="ico" viewBox="0 0 24 24" aria-hidden="true"><path d="M14 3H7a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V8z"/><path d="M14 3v5h5"/></svg>';
                var nameEl = document.createElement('span');
                nameEl.textContent = file.name;
                var sizeEl = document.createElement('small');
                sizeEl.textContent = sizeText(file.size);
                info.appendChild(nameEl);
                info.appendChild(sizeEl);

                var remove = document.createElement('button');
                remove.type = 'button';
                remove.title = 'حذف';
                remove.setAttribute('aria-label', 'حذف ' + file.name);
                remove.innerHTML = '<svg class="ico ico-sm" viewBox="0 0 24 24" aria-hidden="true"><path d="M18 6 6 18M6 6l12 12"/></svg>';
                remove.addEventListener('click', function () {
                    selected.items.remove(index);
                    input.files = selected.files;
                    showErrors([]);
                    render();
                });

                chip.appendChild(info);
                chip.appendChild(remove);
                list.appendChild(chip);
            });
        }
    });
});
