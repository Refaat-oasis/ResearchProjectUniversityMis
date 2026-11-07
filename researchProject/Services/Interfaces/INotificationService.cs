using System.Collections.Generic;
using System.Threading.Tasks;
using researchProject.Models.ViewModels;

namespace researchProject.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string title, string message, string targetUrl, string type, string icon = "bi-bell");
        Task<List<NotificationViewModel>> GetUserNotificationsAsync(string userId, int count = 15);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> MarkAsReadAsync(int notificationId, string userId);
        Task<bool> MarkAllAsReadAsync(string userId);
    }
}

