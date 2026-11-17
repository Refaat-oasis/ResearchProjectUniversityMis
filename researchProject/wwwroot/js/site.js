// Unified Client-Side Routing and AJAX Helpers

$(document).ready(function () {
    // Initialize shared UI helpers unconditionally on load
    initCustomDatalists();
    initSidebarToggle();
    initUniversalTablePagination();

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
});

// Helper for submitting AJAX forms with file support and anti-forgery
function handleAjaxFormSubmit(formElement, successCallback) {
    var $form = $(formElement);
    if (!$form.valid()) {
        return false;
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
                    if (successCallback) {
                        successCallback(response);
                    } else {
                        alert(response.message || 'تمت العملية بنجاح.');
                    }
                } else {
                    alert(response.message || 'حدث خطأ أثناء تنفيذ العملية.');
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
            alert(errMsg);
        }
    });

    return false;
}

// Global Unified Datalist Handler
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
