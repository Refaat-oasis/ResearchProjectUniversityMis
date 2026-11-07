using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using researchProject.Services.Interfaces;

namespace researchProject.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRecent()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            return Json(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Json(new { count });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var success = await _notificationService.MarkAsReadAsync(id, userId);
            return Json(new { success });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var success = await _notificationService.MarkAllAsReadAsync(userId);
            return Json(new { success });
        }
    }
}

