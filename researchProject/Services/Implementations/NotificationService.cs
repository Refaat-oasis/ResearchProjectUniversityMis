using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using researchProject.Data;
using researchProject.Hubs;
using researchProject.Models.Entities;
using researchProject.Models.ViewModels;
using researchProject.Services.Interfaces;

namespace researchProject.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            ApplicationDbContext context,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task SendNotificationAsync(string userId, string title, string message, string targetUrl, string type, string icon = "bi-bell")
        {
            if (string.IsNullOrWhiteSpace(userId)) return;

            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                TargetUrl = targetUrl ?? string.Empty,
                Type = type ?? "General",
                Icon = icon ?? "bi-bell",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var viewModel = new NotificationViewModel
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                TargetUrl = notification.TargetUrl,
                Type = notification.Type,
                Icon = notification.Icon,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                TimeAgo = "الآن"
            };

            // Push in real-time via SignalR to user
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", viewModel);
        }

        public async Task<List<NotificationViewModel>> GetUserNotificationsAsync(string userId, int count = 15)
        {
            var notifications = await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToListAsync();

            return notifications.Select(n => new NotificationViewModel
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                TargetUrl = n.TargetUrl,
                Type = n.Type,
                Icon = n.Icon,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                TimeAgo = FormatArabicTimeAgo(n.CreatedAt)
            }).ToList();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null) return false;

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (unreadNotifications.Count == 0) return true;

            foreach (var n in unreadNotifications)
            {
                n.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private static string FormatArabicTimeAgo(DateTime dateTime)
        {
            var span = DateTime.UtcNow - dateTime;
            if (span.TotalSeconds < 60) return "الآن";
            if (span.TotalMinutes < 60) return $"منذ {(int)span.TotalMinutes} دقيقة";
            if (span.TotalHours < 24) return $"منذ {(int)span.TotalHours} ساعة";
            if (span.TotalDays < 2) return "أمس";
            if (span.TotalDays < 30) return $"منذ {(int)span.TotalDays} يوم";
            return dateTime.ToString("yyyy/MM/dd");
        }
    }
}

