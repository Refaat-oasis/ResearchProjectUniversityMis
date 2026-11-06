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
