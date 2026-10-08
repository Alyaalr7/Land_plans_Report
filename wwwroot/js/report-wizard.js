// خطوات تقرير الإنشاء/التعديل — عرض وتنقل فقط، الحفظ والتحقق النهائي يبقى على السيرفر
(function () {
    var form = document.querySelector('[data-wizard-form]');
    if (!form) return;

    var steps = Array.from(form.querySelectorAll('[data-wizard-step]'));
    var stepNumbers = steps.map(function (s) { return +s.dataset.wizardStep; })
        .filter(function (n, i, a) { return a.indexOf(n) === i; })
        .sort(function (a, b) { return a - b; });

    var stepper = document.querySelector('[data-stepper]');
    var btnPrev = form.querySelector('[data-wizard-prev]');
    var btnNext = form.querySelector('[data-wizard-next]');
    var btnSubmit = form.querySelector('[data-wizard-submit]');
    var dialog = form.querySelector('[data-submit-dialog]');
    var current;

    // ── 1) تنبيه الخروج بدون حفظ ──
    var dirty = false, submitting = false;
    form.addEventListener('input', function () { dirty = true; });
    form.addEventListener('change', function () { dirty = true; });
    var leaving = false;

    // روابط النظام (القائمة، مسار التصفح، إلغاء، تسجيل الخروج): نافذة من النظام
    var leaveDialog = form.querySelector('[data-leave-dialog]');
    var leaveUrl = null;
    document.addEventListener('click', function (e) {
        var a = e.target.closest && e.target.closest('a[href]');
        if (!a || !dirty || submitting || !leaveDialog || !leaveDialog.showModal) return;
        var href = a.getAttribute('href');
        if (!href || href.charAt(0) === '#' || a.target === '_blank' || a.hasAttribute('download')) return;
        if (e.ctrlKey || e.metaKey || e.shiftKey || e.button !== 0) return;
        e.preventDefault();
        leaveUrl = a.href;
        leaveDialog.showModal();
    });
    if (leaveDialog) {
        leaveDialog.querySelector('[data-leave-stay]').addEventListener('click', function () { leaveDialog.close(); });
        leaveDialog.querySelector('[data-leave-go]').addEventListener('click', function () {
            leaving = true;
            leaveDialog.close();
            if (leaveUrl) window.location.href = leaveUrl;
        });
    }

    // إغلاق التبويب أو تحديث الصفحة فقط: المتصفح لا يسمح بغير نافذته
    window.addEventListener('beforeunload', function (e) {
        if (dirty && !submitting && !leaving) { e.preventDefault(); e.returnValue = ''; }
    });

    // ── 2) تأكيد الإرسال (إجراء نهائي) ──
    btnSubmit.addEventListener('click', function () {
        if (current !== undefined && !stepIsValid(current)) return;
        if (dialog && dialog.showModal) dialog.showModal();
        else form.requestSubmit(form.querySelector('[data-submit-dialog] [type=submit]'));
    });
    if (dialog) {
        dialog.querySelector('[data-dialog-close]').addEventListener('click', function () { dialog.close(); });
    }

    // ── 3) منع الضغط مرتين: تعطيل الأزرار بعد بدء الإرسال الفعلي ──
    form.addEventListener('submit', function (e) {
        submitting = true;
        if (dialog && dialog.open) dialog.close();
        setTimeout(function () {
            if (e.defaultPrevented) { submitting = false; return; }   // التحقق رفض الإرسال
            form.querySelectorAll('button').forEach(function (b) { b.disabled = true; });
            var by = e.submitter;
            if (by && by.value === 'submit') btnSubmit.textContent = 'جاري الإرسال…';
            if (by && by.value === 'draft') by.textContent = 'جاري الحفظ…';
        }, 0);
    });

    // ── 4) بعد رجوع الصفحة بخطأ: تنبيه إعادة إرفاق الصور (المتصفح يمسحها) ──
    if (form.querySelector('.validation-summary-errors, .field-validation-error')) {
        var zone = form.querySelector('.file-upload-zone');
        if (zone) {
            var note = document.createElement('div');
            note.className = 'dga-alert dga-alert--warning';
            note.setAttribute('role', 'status');
            note.innerHTML = '<svg class="ico" viewBox="0 0 24 24"><path d="M12 3 2 21h20z"/><path d="M12 10v5"/><circle cx="12" cy="18" r=".6" fill="currentColor"/></svg>'
                + '<div><p>الصور المختارة سابقاً لا تُحفظ عند ظهور خطأ في الصفحة. أعد إرفاقها قبل الإرسال.</p></div>';
            zone.parentElement.insertBefore(note, zone);
        }
    }

    // لو الفورم ما فيه خطوات: نعرض كل شي ونخلي زر الإرسال بس
    if (!stepNumbers.length) {
        if (stepper) stepper.hidden = true;
        btnPrev.hidden = true;
        btnNext.hidden = true;
        return;
    }

    var last = stepNumbers[stepNumbers.length - 1];
    current = stepNumbers[0];

    // التحقق في المتصفح (jQuery Validation): يشمل حقول الموقع المخفية بالذات
    function validator() {
        if (!window.jQuery || !jQuery.fn.validate) return null;
        var v = jQuery(form).validate();
        v.settings.ignore = ':hidden:not([name="Latitude"]):not([name="Longitude"])';
        return v;
    }

    function stepIsValid(n) {
        var v = validator();
        if (!v) return true;
        var ok = true, first = null;
        steps.filter(function (s) { return +s.dataset.wizardStep === n; }).forEach(function (s) {
            s.querySelectorAll('input, select, textarea').forEach(function (el) {
                if (!el.name || el.disabled || el.type === 'file') return;
                if (v.element(el) === false) {
                    ok = false;
                    if (!first) first = el;
                }
            });
        });
        if (first) {
            var target = first.type === 'hidden' ? first.closest('[data-wizard-step]').querySelector('.map-picker') || first : first;
            target.scrollIntoView({ behavior: 'smooth', block: 'center' });
            if (first.type !== 'hidden') first.focus({ preventScroll: true });
        }
        return ok;
    }

    function show(n, scroll) {
        current = n;
        steps.forEach(function (s) { s.hidden = +s.dataset.wizardStep !== n; });

        if (stepper) {
            stepper.querySelectorAll('[data-step-target]').forEach(function (li) {
                var t = +li.dataset.stepTarget;
                li.classList.toggle('is-current', t === n);
                li.classList.toggle('is-done', t < n);
                if (t === n) li.setAttribute('aria-current', 'step'); else li.removeAttribute('aria-current');
                if (t < n) { li.tabIndex = 0; li.setAttribute('role', 'button'); li.setAttribute('aria-label', 'الرجوع إلى خطوة ' + li.textContent.trim()); }
                else { li.removeAttribute('tabindex'); li.removeAttribute('role'); li.removeAttribute('aria-label'); }
            });
        }

        btnPrev.hidden = n === stepNumbers[0];
        btnNext.hidden = n === last;
        btnSubmit.hidden = n !== last;

        if (n === last) buildReview();

        // الخريطة تحتاج تعيد حساب حجمها لما تظهر
        window.dispatchEvent(new Event('resize'));
        if (scroll) (stepper || form).scrollIntoView({ behavior: 'smooth', block: 'start' });
    }

    function go(dir) {
        var i = stepNumbers.indexOf(current) + dir;
        if (i < 0 || i >= stepNumbers.length) return;
        if (dir > 0 && !stepIsValid(current)) return;
        show(stepNumbers[i], true);
    }

    btnNext.addEventListener('click', function () { go(1); });
    btnPrev.addEventListener('click', function () { go(-1); });

    // الرجوع لخطوة سابقة من المؤشر نفسه
    if (stepper) {
        stepper.querySelectorAll('[data-step-target]').forEach(function (li) {
            function back() {
                var t = +li.dataset.stepTarget;
                if (t < current) show(t, true);
            }
            li.addEventListener('click', back);
            li.addEventListener('keydown', function (e) {
                if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); back(); }
            });
        });
    }

    // Enter داخل حقل = "التالي" (بدل ما يحفظ مسودة بالغلط)
    form.addEventListener('keydown', function (e) {
        if (e.key !== 'Enter' || e.target.tagName === 'TEXTAREA' || e.target.tagName === 'BUTTON' || e.target.closest('[role=button]')) return;
        e.preventDefault();
        if (current !== last) go(1);
    });

    // ملخص المراجعة: يقرأ الحقول المعبّاة ويعرضها (عرض فقط)
    function buildReview() {
        var box = form.querySelector('[data-wizard-review]');
        if (!box) return;
        box.innerHTML = '';

        steps.filter(function (s) { return +s.dataset.wizardStep < last; }).forEach(function (s) {
            s.querySelectorAll('.form-section').forEach(function (sec) {
                var h = sec.querySelector('.form-section-header h4');
                var items = [];
                sec.querySelectorAll('.field').forEach(function (f) {
                    if (f.closest('[data-show-when]') && f.closest('[data-show-when]').style.display === 'none') return;
                    var label = f.querySelector(':scope > label');
                    if (!label || f.querySelector('input[type=file]')) return;
                    var value = '';
                    var radio = f.querySelector('input[type=radio]:checked');
                    var select = f.querySelector('select');
                    var input = f.querySelector('input:not([type=radio]):not([type=hidden]), textarea');
                    if (radio) value = radio.parentElement.textContent.trim();
                    else if (select && select.value) value = select.options[select.selectedIndex].text;
                    else if (input) value = input.value.trim();
                    items.push([label.textContent.replace('*', '').trim(), value]);
                });
                var lat = sec.querySelector('input[name="Latitude"]');
                if (lat) items.push(['الموقع الجغرافي', lat.value ? 'محدد على الخريطة' : '']);
                if (!items.length) return;

                var group = document.createElement('section');
                group.className = 'review-group';
                if (h) {
                    var title = document.createElement('h5');
                    title.textContent = h.childNodes[0].textContent.trim();
                    group.appendChild(title);
                }
                var dl = document.createElement('dl');
                dl.className = 'kv-grid';
                items.forEach(function (it) {
                    var d = document.createElement('div');
                    d.className = 'kv' + (it[1] ? '' : ' is-empty');
                    var dt = document.createElement('dt'); dt.textContent = it[0];
                    var dd = document.createElement('dd'); dd.textContent = it[1] || 'لم يُعبّأ';
                    d.appendChild(dt); d.appendChild(dd); dl.appendChild(d);
                });
                group.appendChild(dl);
                box.appendChild(group);
            });
        });

        // الصور
        var newPhotos = form.querySelectorAll('.file-list .file-chip').length;
        var existing = +(form.dataset.existingPhotos || 0);
        var p = document.createElement('section');
        p.className = 'review-group';
        p.innerHTML = '<h5>المرفقات</h5>';
        var dl2 = document.createElement('dl');
        dl2.className = 'kv-grid';
        var total = newPhotos + existing;
        dl2.innerHTML = '<div class="kv' + (total ? '' : ' is-empty') + '"><dt>عدد الصور</dt><dd></dd></div>';
        dl2.querySelector('dd').textContent = total
            ? total + (existing ? ' (منها ' + existing + ' محفوظة سابقاً)' : '')
            : 'لا توجد صور — مطلوبة عند الإرسال';
        p.appendChild(dl2);
        box.appendChild(p);
    }

    // لما يتحدد الموقع على الخريطة يختفي خطأ "حدد الموقع" فوراً
    form.querySelectorAll('[id$="_latLabel"]').forEach(function (lbl) {
        new MutationObserver(function () {
            var v = validator();
            var lat = form.querySelector('input[name="Latitude"]');
            if (v && lat && lat.value) v.element(lat);
        }).observe(lbl, { childList: true, characterData: true, subtree: true });
    });

    // بعد رجوع الصفحة من السيرفر بأخطاء: نفتح أول خطوة فيها خطأ
    var firstError = form.querySelector('.input-validation-error, .field-validation-error');
    var startStep = stepNumbers[0];
    if (firstError && firstError.closest('[data-wizard-step]')) {
        startStep = +firstError.closest('[data-wizard-step]').dataset.wizardStep;
    }

    // ننتظر تجهيز التحقق (jQuery) ثم نبدأ
    if (window.jQuery) jQuery(function () { validator(); show(startStep); });
    else show(startStep);
})();
