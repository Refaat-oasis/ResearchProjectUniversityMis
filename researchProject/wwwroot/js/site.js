// Unified Client-Side Routing and AJAX Helpers

$(document).ready(function () {
    // Initialize shared UI helpers unconditionally on load
    initCustomDatalists();
    initSidebarToggle();
    initUniversalTablePagination();
    initAnimatedElements();

    // 1. Sidebar link click handling
    $(document).on('click', '.sidebar-item a[data-url]', function (e) {
        e.preventDefault();
        var url = $(this).data('url');
        var hash = $(this).data('hash');
        loadPartial(url, '#main-content-container', hash);
    });

    // 2. Initial route resolution based on window.location.hash
    function resolveInitialRoute() {
        var hash = window.location.hash.replace('#', '');
        if (hash) {
            var $targetLink = $('.sidebar-item a[data-hash="' + hash + '"]');
            if ($targetLink.length) {
                $targetLink.trigger('click');
                return;
            }
        }

        // If no hash or match, load the first active or first available link
        var $firstLink = $('.sidebar-item a.active').first();
        if (!$firstLink.length) {
            $firstLink = $('.sidebar-item a[data-url]').first();
        }

        if ($firstLink.length && $('#main-content-container').is(':empty')) {
            $firstLink.trigger('click');
        }

        // Initialize Dedicated Dropdowns & Autocomplete
        initDedicatedDropdowns();
        initCustomDatalists();
    }

    resolveInitialRoute();

    // 3. Listen to browser history navigation (back/forward)
    window.addEventListener('hashchange', function () {
        var hash = window.location.hash.replace('#', '');
        if (hash) {
            var $link = $('.sidebar-item a[data-hash="' + hash + '"]');
            if ($link.length && !$link.hasClass('active')) {
                loadPartial($link.data('url'), '#main-content-container', hash);
            }
        }
    });

    // Close any open dedicated dropdowns on outside click
    $(document).on('click', function (e) {
        if (!$(e.target).closest('.dedicated-dropdown-wrapper').length) {
            $('.dedicated-dropdown-wrapper').removeClass('open');
        }
    });
});

// ==========================================================================
// 1. Global Top-Right Toast Notification System (Requirement 1)
// ==========================================================================
window.showToast = function (firstArg, secondArg) {
    var isSuccess = false;
    var message = '';

    if (typeof firstArg === 'boolean') {
        isSuccess = firstArg;
        message = secondArg;
    } else if (firstArg === 'success' || firstArg === 'error') {
        isSuccess = (firstArg === 'success');
        message = secondArg;
    } else if (secondArg === 'success' || secondArg === 'error') {
        isSuccess = (secondArg === 'success');
        message = firstArg;
    } else if (typeof secondArg === 'boolean') {
        isSuccess = secondArg;
        message = firstArg;
    } else {
        message = firstArg || secondArg || '';
        var str = String(message);
        isSuccess = str.indexOf('بنجاح') !== -1 || str.indexOf('تم ') === 0;
    }

    var defaultMsg = isSuccess ? 'تم الحفظ بنجاح.' : 'حدث خطأ ما، يرجى المحاولة مرة أخرى.';
    var text = message || defaultMsg;

    // Sanitize any raw server error HTML payloads
    if (typeof text === 'string') {
        var trimmed = text.trim();
        if (trimmed.startsWith('<!DOCTYPE') || trimmed.startsWith('<html') || trimmed.indexOf('Server Error') !== -1 || trimmed.indexOf('404 - File') !== -1) {
            if (trimmed.indexOf('404') !== -1) {
                text = 'تعذر العثور على المسار أو المورد المطلوب في الخادم (404 Not Found).';
            } else if (trimmed.indexOf('400') !== -1) {
                text = 'فشل التحقق من صحة الطلب أو رمز الأمان (400 Bad Request).';
            } else if (trimmed.indexOf('401') !== -1 || trimmed.indexOf('403') !== -1) {
                text = 'ليس لديك الصلاحية لتنفيذ هذا الإجراء أو انتهت جلستك الحالية.';
            } else {
                text = 'حدث خطأ في استجابة الخادم. يرجى إعادة المحاولة.';
            }
        }
    }

    var $container = $('#toast-container');
    if (!$container.length) {
        $('body').append('<div id="toast-container" class="app-toast-container" aria-live="polite" aria-atomic="true"></div>');
        $container = $('#toast-container');
    }

    var iconClass = isSuccess ? 'bi bi-check-circle-fill' : 'bi bi-exclamation-triangle-fill';
    var toastClass = isSuccess ? 'app-toast-success' : 'app-toast-error';

    var $toast = $(
        '<div class="app-toast ' + toastClass + '" role="alert">' +
            '<i class="' + iconClass + ' app-toast-icon"></i>' +
            '<div class="app-toast-body">' + $('<div>').text(text).html() + '</div>' +
            '<button type="button" class="app-toast-close" aria-label="Close">' +
                '<i class="bi bi-x-lg"></i>' +
            '</button>' +
        '</div>'
    );

    $container.append($toast);

    // Smooth entrance
    setTimeout(function () {
        $toast.addClass('show');
    }, 15);

    // Auto dismiss after 3.5 seconds
    var dismissTimer = setTimeout(function () {
        dismiss();
    }, 3500);

    function dismiss() {
        clearTimeout(dismissTimer);
        $toast.removeClass('show');
        setTimeout(function () {
            $toast.remove();
        }, 320);
    }

    $toast.find('.app-toast-close').on('click', function (e) {
        e.preventDefault();
        dismiss();
    });
};

// ==========================================================================
// Global Safe Clipboard Copy Helper with ExecCommand Fallback
// ==========================================================================
window.safeCopyToClipboard = function (text, onSuccess, onError) {
    if (!text) return;
    try {
        if (navigator.clipboard && window.isSecureContext && typeof navigator.clipboard.writeText === 'function') {
            navigator.clipboard.writeText(text).then(function () {
                if (typeof onSuccess === 'function') onSuccess();
            }).catch(function () {
                fallbackCopy(text, onSuccess, onError);
            });
            return;
        }
    } catch (e) { }

    fallbackCopy(text, onSuccess, onError);

    function fallbackCopy(val, onOk, onFail) {
        try {
            var textArea = document.createElement("textarea");
            textArea.value = val;
            textArea.style.position = "fixed";
            textArea.style.top = "-9999px";
            textArea.style.left = "-9999px";
            textArea.setAttribute("readonly", "");
            document.body.appendChild(textArea);
            textArea.focus();
            textArea.select();
            var success = false;
            try {
                success = document.execCommand('copy');
            } catch (err) {
                success = false;
            }
            document.body.removeChild(textArea);
            if (success) {
                if (typeof onOk === 'function') onOk();
                return;
            }
        } catch (ex) { }

        if (typeof onFail === 'function') {
            onFail();
        } else {
            window.prompt('يرجى نسخ الرمز (Ctrl+C):', val);
        }
    }
};

// Global window.alert override as a universal fallback guard
window.alert = function (msg) {
    if (!msg) return;
    var str = String(msg);
    var isSuccess = str.indexOf('بنجاح') !== -1 || str.indexOf('تم ') === 0;
    window.showToast(isSuccess ? 'success' : 'error', str);
};

// Helper for submitting AJAX forms with file support and anti-forgery
function handleAjaxFormSubmit(formElement, successCallback) {
    var $form = $(formElement);

    // Sync any custom inputs in dedicated dropdowns before form validation/submission
    $form.find('.dedicated-dropdown-wrapper').each(function () {
        var $wrap = $(this);
        var $txtInput = $wrap.find('.dedicated-dropdown-input');
        var hiddenSelector = $wrap.data('target-input');
        var $hid = hiddenSelector ? $(hiddenSelector) : $wrap.find('input[type="hidden"]');
        if ($txtInput.length && $hid.length && $txtInput.val()) {
            $hid.val($txtInput.val().trim());
        }
    });

    if (!$form.valid()) {
        return false;
    }

    // Check for empty 0-byte files before sending
    var fileInputs = $form.find('input[type="file"]');
    for (var i = 0; i < fileInputs.length; i++) {
        var files = fileInputs[i].files;
        if (files && files.length > 0) {
            if (files[0].size === 0) {
                window.showToast('error', 'الملف المرفق (' + files[0].name + ') فارغ (حجمه 0 بايت). يرجى التأكد من اختيار ملف صالح يحتوي على محتوى.');
                return false;
            }
        }
    }

    var formData = new FormData(formElement);
    var actionUrl = $form.attr('action');
    var method = $form.attr('method') || 'POST';

    $('#partial-loading-spinner').css('display', 'flex');

    $.ajax({
        url: actionUrl,
        type: method,
        data: formData,
        processData: false,
        contentType: false,
        headers: {
            'X-CSRF-TOKEN': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            $('#partial-loading-spinner').hide();
            if (response && response.success !== undefined) {
                if (response.success) {
                    window.showToast('success', response.message || 'تم الحفظ بنجاح.');
                    if (successCallback) {
                        successCallback(response);
                    }
                } else {
                    window.showToast('error', response.message || 'حدث خطأ أثناء تنفيذ العملية.');
                }
            } else {
                if (successCallback) successCallback(response);
            }
        },
        error: function (xhr) {
            $('#partial-loading-spinner').hide();
            var errMsg = 'حدث خطأ في الاتصال بالخادم.';
            try {
                var json = JSON.parse(xhr.responseText);
                if (json && json.message) errMsg = json.message;
            } catch (e) {
                if (xhr.status === 404) {
                    errMsg = 'تعذر العثور على المسار المطلوب في الخادم (404 Not Found).';
                } else if (xhr.status === 400) {
                    errMsg = 'فشل التحقق من صحة البيانات أو رمز الأمان (400 Bad Request).';
                } else if (xhr.status === 403 || xhr.status === 401) {
                    errMsg = 'ليس لديك الصلاحية لتنفيذ هذا الإجراء أو انتهت جلستك الحالية.';
                } else if (xhr.status >= 500) {
                    errMsg = 'حدث خطأ غير متوقع في الخادم (500 Server Error).';
                } else if (xhr.responseText && !xhr.responseText.trim().startsWith('<')) {
                    errMsg = xhr.responseText;
                }
            }
            window.showToast('error', errMsg);
        }
    });

    return false;
}

// ==========================================================================
// 2. Dedicated Dropdown & Autocomplete Component Engine (Requirement 2 & 6)
// ==========================================================================
function initDedicatedDropdowns(container) {
    var $root = container ? $(container) : $(document);

    $root.find('.dedicated-dropdown-wrapper').each(function () {
        var $wrapper = $(this);
        if ($wrapper.data('initialized')) return;
        $wrapper.data('initialized', true);

        var $input = $wrapper.find('.dedicated-dropdown-input');
        var $clearBtn = $wrapper.find('.dedicated-dropdown-clear-btn');
        var $chevronBtn = $wrapper.find('.dedicated-dropdown-chevron-btn');
        var $menu = $wrapper.find('.dedicated-dropdown-menu');
        function getItems() {
            return $wrapper.find('.dedicated-dropdown-item');
        }
        var hiddenSelector = $wrapper.data('target-input');
        var $hiddenInput = hiddenSelector ? $(hiddenSelector) : $wrapper.find('input[type="hidden"]');

        function toggleClearBtn() {
            if ($input.val() && $input.val().length > 0) {
                $clearBtn.show();
            } else {
                $clearBtn.hide();
            }
        }

        function filterOptions(term) {
            var search = (term || '').trim().toLowerCase();
            var visibleCount = 0;
            var $currentItems = getItems();

            $currentItems.each(function () {
                var $item = $(this);
                var text = ($item.data('label') || $item.data('name') || $item.text() || '').toLowerCase();
                if (!search || text.indexOf(search) !== -1) {
                    $item.show();
                    visibleCount++;
                } else {
                    $item.hide();
                }
            });

            var $emptyMsg = $menu.find('.dedicated-dropdown-empty');
            if (visibleCount === 0) {
                if (!$emptyMsg.length) {
                    $menu.append('<div class="dedicated-dropdown-empty"><i class="bi bi-search me-1"></i> لا توجد نتائج مطابقة</div>');
                } else {
                    $emptyMsg.show();
                }
            } else {
                $emptyMsg.hide();
            }
        }

        // Pre-select if hidden input already has a value
        if ($hiddenInput.length && $hiddenInput.val()) {
            var currentVal = $hiddenInput.val();
            var $currentItems = getItems();
            var $matched = $currentItems.filter(function () {
                var itemVal = $(this).data('value') !== undefined ? $(this).data('value') : $(this).data('id');
                return String(itemVal) === String(currentVal);
            });
            if ($matched.length) {
                $currentItems.removeClass('selected');
                $matched.addClass('selected');
                $input.val(($matched.data('label') || $matched.data('name') || $matched.text()).trim());
            }
        }

        toggleClearBtn();

        // Open menu on input focus or click
        $input.on('focus click', function () {
            $('.dedicated-dropdown-wrapper').not($wrapper).removeClass('open');
            $wrapper.addClass('open');
            filterOptions($input.val());
        });

        // Typing in input filters options
        $input.on('input', function () {
            toggleClearBtn();
            $wrapper.addClass('open');
            filterOptions($(this).val());

            var allowCustom = $wrapper.data('allow-custom') === true || $wrapper.data('allow-custom') === 'true';
            if (allowCustom && $hiddenInput.length) {
                $hiddenInput.val($(this).val().trim());
            } else if (!$(this).val()) {
                // If user cleared text completely, clear hidden ID
                if ($hiddenInput.length) {
                    $hiddenInput.val('').trigger('change');
                }
                getItems().removeClass('selected');
            }
        });

        // Blur event to ensure custom input synchronization
        $input.on('blur', function () {
            var allowCustom = $wrapper.data('allow-custom') === true || $wrapper.data('allow-custom') === 'true';
            if (allowCustom && $hiddenInput.length) {
                $hiddenInput.val($(this).val().trim());
            }
        });

        // Toggle button click
        $chevronBtn.on('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            if ($wrapper.hasClass('open')) {
                $wrapper.removeClass('open');
            } else {
                $('.dedicated-dropdown-wrapper').not($wrapper).removeClass('open');
                $wrapper.addClass('open');
                filterOptions($input.val());
                $input.focus();
            }
        });

        // Clear button click
        $clearBtn.on('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            $input.val('');
            if ($hiddenInput.length) {
                $hiddenInput.val('').trigger('change');
            }
            getItems().removeClass('selected');
            toggleClearBtn();
            filterOptions('');
            $input.focus();
        });

        // Item selection
        $wrapper.on('click', '.dedicated-dropdown-item', function (e) {
            e.preventDefault();
            var $item = $(this);
            var id = $item.data('value') !== undefined ? $item.data('value') : $item.data('id');
            var name = ($item.data('label') || $item.data('name') || $item.text() || '').trim();

            getItems().removeClass('selected');
            $item.addClass('selected');

            $input.val(name);
            if ($hiddenInput.length) {
                $hiddenInput.val(id !== undefined ? id : name).trigger('change');
            }

            toggleClearBtn();
            $wrapper.removeClass('open');
            $input.trigger('change');
        });

        // Keyboard navigation
        $input.on('keydown', function (e) {
            var $currentItems = getItems();
            if (e.key === 'Escape') {
                $wrapper.removeClass('open');
            } else if (e.key === 'ArrowDown') {
                $wrapper.addClass('open');
                var $visible = $currentItems.filter(':visible');
                var $active = $visible.filter('.active');
                if (!$active.length) {
                    $visible.first().addClass('active');
                } else {
                    $active.removeClass('active');
                    var next = $visible.index($active) + 1;
                    if (next < $visible.length) {
                        $visible.eq(next).addClass('active');
                    } else {
                        $visible.first().addClass('active');
                    }
                }
            } else if (e.key === 'ArrowUp') {
                var $visible = $currentItems.filter(':visible');
                var $active = $visible.filter('.active');
                if ($active.length) {
                    $active.removeClass('active');
                    var prev = $visible.index($active) - 1;
                    if (prev >= 0) {
                        $visible.eq(prev).addClass('active');
                    } else {
                        $visible.last().addClass('active');
                    }
                }
            } else if (e.key === 'Enter') {
                var $active = $currentItems.filter(':visible.active');
                if ($active.length) {
                    e.preventDefault();
                    $active.trigger('click');
                }
            }
        });
    });
}

// Global Unified Datalist Handler (ui-taste & frontend-design)
function initCustomDatalists(container) {
    var $root = container ? $(container) : $(document);
    $root.find('.custom-datalist-wrapper').each(function () {
        var $wrapper = $(this);
        var $input = $wrapper.find('.custom-datalist-input');
        var $clearBtn = $wrapper.find('.datalist-clear-btn');
        var hiddenTarget = $input.data('hidden-target');
        var listId = $input.attr('list');

        function toggleClearBtn() {
            if ($input.val() && $input.val().length > 0) {
                $clearBtn.show();
            } else {
                $clearBtn.hide();
            }
        }

        toggleClearBtn();

        $input.off('input.datalist change.datalist').on('input.datalist change.datalist', function () {
            toggleClearBtn();
            if (hiddenTarget && listId) {
                var currentVal = $(this).val();
                var $matchedOption = $('#' + listId + ' option').filter(function () {
                    return this.value === currentVal;
                });
                if ($matchedOption.length > 0) {
                    var dataId = $matchedOption.data('id');
                    $(hiddenTarget).val(dataId !== undefined ? dataId : currentVal).trigger('change');
                } else {
                    $(hiddenTarget).val('').trigger('change');
                }
            }
        });

        $clearBtn.off('click.datalist').on('click.datalist', function (e) {
            e.preventDefault();
            $input.val('').trigger('input').trigger('change');
            if (hiddenTarget) {
                $(hiddenTarget).val('').trigger('change');
            }
            $input.focus();
        });
    });
}

// 5. Sidebar Toggle & Responsive Drawer Handler
function initSidebarToggle() {
    var isCollapsed = localStorage.getItem('sidebar_collapsed') === 'true';
    if (isCollapsed && $(window).width() >= 992) {
        $('.app-container').addClass('sidebar-collapsed');
    }

    $(document).off('click', '#sidebarToggleBtn').on('click', '#sidebarToggleBtn', function (e) {
        e.preventDefault();
        if ($(window).width() < 992) {
            $('.right-sidebar').toggleClass('show-mobile');
            $('#sidebar-overlay').toggleClass('active');
        } else {
            $('.app-container').toggleClass('sidebar-collapsed');
            var collapsedNow = $('.app-container').hasClass('sidebar-collapsed');
            localStorage.setItem('sidebar_collapsed', collapsedNow ? 'true' : 'false');
        }
    });

    $(document).off('click', '#sidebar-overlay').on('click', '#sidebar-overlay', function () {
        if ($(window).width() < 992) {
            $('.right-sidebar').removeClass('show-mobile');
            $('#sidebar-overlay').removeClass('active');
        }
    });

    $(document).off('keydown.sidebarEsc').on('keydown.sidebarEsc', function (e) {
        if (e.key === 'Escape' && $(window).width() < 992) {
            $('.right-sidebar').removeClass('show-mobile');
            $('#sidebar-overlay').removeClass('active');
        }
    });
}

// 6. Universal Table Pagination Engine with Page Size (10, 25, 50, All)
function initUniversalTablePagination(container) {
    var $root = container ? $(container) : $(document);

    $root.find('table.table').each(function () {
        var $table = $(this);
        if ($table.hasClass('table-paginated-initialized') || $table.hasClass('no-pagination')) return;

        var $tbody = $table.find('tbody');
        var $allRows = $tbody.find('tr').filter(function () {
            return $(this).find('td[colspan]').length === 0;
        });

        var totalRows = $allRows.length;
        if (totalRows <= 5) return; // Keep small tables clean without extra controls

        $table.addClass('table-paginated-initialized');
        var tableId = 'table-' + Math.random().toString(36).substring(2, 9);
        $table.attr('id', tableId);

        var currentPage = 1;
        var pageSize = 10; // Default: 10 rows

        var controlsHtml = `
            <div class="pagination-wrapper" data-table-id="${tableId}">
                <div class="page-size-selector">
                    <label class="small text-muted mb-0">عرض:</label>
                    <select class="form-select form-select-sm page-size-select">
                        <option value="10" selected>10 صفوف</option>
                        <option value="25">25 صفاً</option>
                        <option value="50">50 صفاً</option>
                        <option value="all">عرض الكل (${totalRows})</option>
                    </select>
                    <span class="pagination-info ms-2">إجمالي: <strong class="font-mono-tabular">${totalRows}</strong> سجل</span>
                </div>
                <nav aria-label="تنقل الصفحات">
                    <ul class="pagination pagination-sm mb-0">
                    </ul>
                </nav>
            </div>
        `;

        var $wrapper = $(controlsHtml);
        var $container = $table.closest('.table-responsive');
        if ($container.length) {
            $container.after($wrapper);
        } else {
            $table.after($wrapper);
        }

        function renderPage() {
            var actualPageSize = (pageSize === 'all') ? totalRows : parseInt(pageSize, 10);
            var totalPages = Math.ceil(totalRows / actualPageSize) || 1;

            if (currentPage > totalPages) currentPage = totalPages;
            if (currentPage < 1) currentPage = 1;

            var startIndex = (currentPage - 1) * actualPageSize;
            var endIndex = (pageSize === 'all') ? totalRows : Math.min(startIndex + actualPageSize, totalRows);

            $allRows.hide();
            $allRows.slice(startIndex, endIndex).show();

            var infoText = (totalRows === 0)
                ? 'لا توجد بيانات'
                : `عرض <span class="font-mono-tabular">${startIndex + 1}</span> إلى <span class="font-mono-tabular">${endIndex}</span> من إجمالي <strong class="font-mono-tabular">${totalRows}</strong> سجل`;
            $wrapper.find('.pagination-info').html(infoText);

            var $paginationUl = $wrapper.find('.pagination');
            $paginationUl.empty();

            if (totalPages <= 1) return;

            var prevDisabled = (currentPage === 1) ? 'disabled' : '';
            $paginationUl.append(`
                <li class="page-item ${prevDisabled}">
                    <a class="page-link btn-prev" href="#" aria-label="السابق" title="السابق">
                        <i class="bi bi-chevron-right"></i>
                    </a>
                </li>
            `);

            for (var p = 1; p <= totalPages; p++) {
                if (p === 1 || p === totalPages || (p >= currentPage - 2 && p <= currentPage + 2)) {
                    var activeClass = (p === currentPage) ? 'active' : '';
                    $paginationUl.append(`
                        <li class="page-item ${activeClass}">
                            <a class="page-link btn-page font-mono-tabular" href="#" data-page="${p}">${p}</a>
                        </li>
                    `);
                } else if (p === currentPage - 3 || p === currentPage + 3) {
                    $paginationUl.append('<li class="page-item disabled"><span class="page-link font-mono-tabular">...</span></li>');
                }
            }

            var nextDisabled = (currentPage === totalPages) ? 'disabled' : '';
            $paginationUl.append(`
                <li class="page-item ${nextDisabled}">
                    <a class="page-link btn-next" href="#" aria-label="التالي" title="التالي">
                        <i class="bi bi-chevron-left"></i>
                    </a>
                </li>
            `);
        }

        $wrapper.on('change', '.page-size-select', function () {
            pageSize = $(this).val();
            currentPage = 1;
            renderPage();
        });

        $wrapper.on('click', '.btn-page', function (e) {
            e.preventDefault();
            currentPage = parseInt($(this).data('page'), 10);
            renderPage();
        });

        $wrapper.on('click', '.btn-prev', function (e) {
            e.preventDefault();
            if (currentPage > 1) {
                currentPage--;
                renderPage();
            }
        });

        $wrapper.on('click', '.btn-next', function (e) {
            e.preventDefault();
            var actualPageSize = (pageSize === 'all') ? totalRows : parseInt(pageSize, 10);
            var totalPages = Math.ceil(totalRows / actualPageSize);
            if (currentPage < totalPages) {
                currentPage++;
                renderPage();
            }
        });

        renderPage();
    });
}

// ==========================================================================
// Universal Count-Up and Progress Bar Animation System
// ==========================================================================
window.initAnimatedElements = function (container) {
    var $root = container ? $(container) : $('body');
    if (!$root.length) return;

    var prefersReducedMotion = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    // Helper: Animate Single Progress Bar
    function animateProgressBar(bar) {
        var $bar = $(bar);
        if ($bar.data('animated-bar-done')) return;
        $bar.data('animated-bar-done', true);

        var targetWidth = $bar.attr('data-target-width') || $bar[0].style.width;
        if (!targetWidth || targetWidth === '0px' || targetWidth === '0%') {
            return;
        }

        if (prefersReducedMotion) {
            $bar.css('width', targetWidth);
            return;
        }

        // Start from 0% and smoothly transition to targetWidth
        $bar.css({
            'width': '0%',
            'transition': 'none'
        });

        // Trigger reflow then apply transition & width
        requestAnimationFrame(function () {
            requestAnimationFrame(function () {
                $bar.addClass('progress-bar-animated-fill');
                $bar.css({
                    'width': targetWidth,
                    'transition': 'width 1.35s cubic-bezier(0.16, 1, 0.3, 1)'
                });
            });
        });
    }

    // Helper: Animate Single Counter Number
    function animateCounter(el) {
        var $el = $(el);
        if ($el.data('animated-counter-done')) return;
        $el.data('animated-counter-done', true);

        var rawTarget = $el.attr('data-target');
        var suffix = $el.attr('data-suffix') || '';
        var prefix = $el.attr('data-prefix') || '';
        var decimalsAttr = $el.attr('data-decimals');

        var target = 0;
        var decimals = 0;

        if (rawTarget !== undefined && rawTarget !== null && rawTarget !== '') {
            target = parseFloat(rawTarget);
            if (decimalsAttr !== undefined && decimalsAttr !== null && decimalsAttr !== '') {
                decimals = parseInt(decimalsAttr, 10);
            } else if (rawTarget.indexOf('.') !== -1) {
                decimals = rawTarget.split('.')[1].length;
            }
        } else {
            var text = $el.text().trim();
            if (text.endsWith('%')) {
                suffix = '%';
                text = text.slice(0, -1).trim();
            }
            if (text.indexOf('.') !== -1) {
                decimals = text.split('.')[1].length;
            }
            target = parseFloat(text);
        }

        if (isNaN(target)) {
            return;
        }

        if (prefersReducedMotion || target === 0) {
            $el.text(prefix + target.toFixed(decimals) + suffix);
            return;
        }

        if (!$el.hasClass('font-mono-tabular')) {
            $el.addClass('font-mono-tabular');
        }

        var duration = 1400; // ms
        var startValue = 0;
        var startTime = null;

        function easeOutExpo(x) {
            return x === 1 ? 1 : 1 - Math.pow(2, -10 * x);
        }

        function step(timestamp) {
            if (!startTime) startTime = timestamp;
            var elapsed = timestamp - startTime;
            var progress = Math.min(elapsed / duration, 1);
            var easedProgress = easeOutExpo(progress);

            var current = startValue + (target - startValue) * easedProgress;
            $el.text(prefix + current.toFixed(decimals) + suffix);

            if (progress < 1) {
                requestAnimationFrame(step);
            } else {
                $el.text(prefix + target.toFixed(decimals) + suffix);
            }
        }

        requestAnimationFrame(function () {
            requestAnimationFrame(step);
        });
    }

    // Process Progress Bars
    var $progressBars = $root.find('.progress-bar[data-target-width], .animated-progress-bar');
    $progressBars.each(function () {
        var bar = this;
        if ('IntersectionObserver' in window) {
            var observer = new IntersectionObserver(function (entries, obs) {
                entries.forEach(function (entry) {
                    if (entry.isIntersecting) {
                        animateProgressBar(entry.target);
                        obs.unobserve(entry.target);
                    }
                });
            }, { threshold: 0.1 });
            observer.observe(bar);
        } else {
            animateProgressBar(bar);
        }
    });

    // Process Counter Numbers
    var $counters = $root.find('[data-counter], .stat-number[data-target], .kpi-ledger-value[data-target], .animated-counter');
    $counters.each(function () {
        var el = this;
        if ('IntersectionObserver' in window) {
            var observer = new IntersectionObserver(function (entries, obs) {
                entries.forEach(function (entry) {
                    if (entry.isIntersecting) {
                        animateCounter(entry.target);
                        obs.unobserve(entry.target);
                    }
                });
            }, { threshold: 0.1 });
            observer.observe(el);
        } else {
            animateCounter(el);
        }
    });
};
