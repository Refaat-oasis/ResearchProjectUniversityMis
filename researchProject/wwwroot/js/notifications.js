// SignalR Client & Interactive Notification Center
(function () {
    let connection = null;

    function initSignalR() {
        if (typeof signalR === 'undefined') {
            console.warn('SignalR library not loaded yet.');
            return;
        }

        connection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/notifications")
            .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
            .build();

        connection.on("ReceiveNotification", function (notif) {
            handleIncomingNotification(notif);
        });

        connection.start()
            .then(function () {
                console.log("SignalR NotificationHub connected.");
            })
            .catch(function (err) {
                console.error("SignalR connection error:", err.toString());
            });
    }

    function playSoftBeep() {
        try {
            const ctx = new (window.AudioContext || window.webkitAudioContext)();
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();
            osc.type = "sine";
            osc.frequency.setValueAtTime(587.33, ctx.currentTime); // D5
            gain.gain.setValueAtTime(0.08, ctx.currentTime);
            gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.35);
            osc.connect(gain);
            gain.connect(ctx.destination);
            osc.start();
            osc.stop(ctx.currentTime + 0.35);
        } catch (e) {
            // Audio context might be restricted before user gesture
        }
    }

    function updateBadge(count) {
        const badge = $('#notifBadge');
        if (count > 0) {
            badge.text(count > 99 ? '99+' : count).show();
        } else {
            badge.hide().text('0');
        }
    }

    function loadUnreadCount() {
        $.get('/Notification/GetUnreadCount', function (res) {
            if (res && typeof res.count === 'number') {
                updateBadge(res.count);
            }
        });
    }

    function loadRecentNotifications() {
        const list = $('#notifList');
        $.get('/Notification/GetRecent', function (items) {
            list.empty();
            if (!items || items.length === 0) {
                list.html(`
                    <div class="notif-empty-state">
                        <i class="bi bi-bell-slash"></i>
                        <div>لا توجد إشعارات حتى الآن</div>
                    </div>
                `);
                return;
            }

            items.forEach(function (n) {
                list.append(buildNotificationItemHtml(n));
            });
        });
    }

    function buildNotificationItemHtml(n) {
        const unreadCls = n.isRead ? '' : 'unread';
        const icon = n.icon || 'bi-bell';
        const targetUrl = n.targetUrl || '#';

        return `
            <div class="notif-item ${unreadCls}" data-id="${n.id}" data-url="${targetUrl}" role="button">
                <div class="notif-icon-box">
                    <i class="bi ${icon}"></i>
                </div>
                <div class="notif-content">
                    <div class="notif-title">${escapeHtml(n.title)}</div>
                    <div class="notif-desc">${escapeHtml(n.message)}</div>
                    <div class="notif-time">
                        <i class="bi bi-clock"></i> ${escapeHtml(n.timeAgo || 'الآن')}
                    </div>
                </div>
            </div>
        `;
    }

    function escapeHtml(text) {
        if (!text) return '';
        return String(text)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    function handleIncomingNotification(notif) {
        if (!notif || !notif.id) return;
        // Guard against duplicate processing
        if ($(`.notif-item[data-id="${notif.id}"]`).length > 0) {
            return;
        }

        playSoftBeep();

        // Increment badge count
        const currentCount = parseInt($('#notifBadge').text()) || 0;
        updateBadge(currentCount + 1);

        // Prepend to dropdown list if open or present
        const list = $('#notifList');
        list.find('.notif-empty-state').remove();
        list.prepend(buildNotificationItemHtml(notif));

        // Show floating toast alert
        showFloatingToast(notif);
    }

    function showFloatingToast(notif) {
        let container = $('#notif-toast-container');
        if (container.length === 0) {
            $('body').append('<div id="notif-toast-container"></div>');
            container = $('#notif-toast-container');
        }

        const icon = notif.icon || 'bi-bell-fill';
        const toast = $(`
            <div class="liquid-glass-toast" role="alert">
                <div class="d-flex align-items-center gap-2 mb-1">
                    <i class="bi ${icon} text-primary fs-5"></i>
                    <strong class="text-dark small">${escapeHtml(notif.title)}</strong>
                </div>
                <div class="small text-muted">${escapeHtml(notif.message)}</div>
            </div>
        `);

        toast.on('click', function () {
            markAndNavigate(notif.id, notif.targetUrl);
        });

        container.prepend(toast);

        setTimeout(function () {
            toast.fadeOut(400, function () { $(this).remove(); });
        }, 6000);
    }

    function markAndNavigate(id, url) {
        const token = $('input[name="__RequestVerificationToken"]').val();
        if (id) {
            $.ajax({
                url: '/Notification/MarkAsRead/' + id,
                type: 'POST',
                headers: { 'X-CSRF-TOKEN': token },
                complete: function () {
                    if (url && url !== '#' && url !== '') {
                        window.location.href = url;
                    }
                }
            });
        } else if (url && url !== '#' && url !== '') {
            window.location.href = url;
        }
    }

    // Document Ready Event Bindings
    $(function () {
        // Only run for authenticated sessions where notifBell exists
        if ($('#notifBellBtn').length > 0) {
            initSignalR();
            loadUnreadCount();

            // When dropdown opens, load recent notifications
            $('#notifDropdown').on('show.bs.dropdown', function () {
                loadRecentNotifications();
            });

            // Pressable notification click handler (event delegation)
            $(document).on('click', '.notif-item', function (e) {
                e.preventDefault();
                const id = $(this).data('id');
                const url = $(this).data('url');
                markAndNavigate(id, url);
            });

            // Mark all as read button
            $(document).on('click', '#notifMarkAllBtn', function (e) {
                e.preventDefault();
                const token = $('input[name="__RequestVerificationToken"]').val();
                $.ajax({
                    url: '/Notification/MarkAllAsRead',
                    type: 'POST',
                    headers: { 'X-CSRF-TOKEN': token },
                    success: function () {
                        updateBadge(0);
                        $('.notif-item').removeClass('unread');
                    }
                });
            });
        }
    });
})();

