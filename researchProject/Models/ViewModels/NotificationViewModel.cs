using System;

namespace researchProject.Models.ViewModels
{
    public class NotificationViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string TargetUrl { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
    }
}

