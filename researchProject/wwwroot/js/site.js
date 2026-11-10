// Unified Client-Side Routing and AJAX Helpers

$(document).ready(function () {
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
                if (xhr.responseText) errMsg = xhr.responseText;
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
        var $items = $wrapper.find('.dedicated-dropdown-item');
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

            $items.each(function () {
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
            var $matched = $items.filter(function () {
                var itemVal = $(this).data('value') !== undefined ? $(this).data('value') : $(this).data('id');
                return String(itemVal) === String(currentVal);
            });
            if ($matched.length) {
                $items.removeClass('selected');
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
                $items.removeClass('selected');
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
            $items.removeClass('selected');
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

            $items.removeClass('selected');
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
            if (e.key === 'Escape') {
                $wrapper.removeClass('open');
            } else if (e.key === 'ArrowDown') {
                $wrapper.addClass('open');
                var $visible = $items.filter(':visible');
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
                var $visible = $items.filter(':visible');
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
                var $active = $items.filter(':visible.active');
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

