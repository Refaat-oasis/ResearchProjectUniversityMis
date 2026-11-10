using System;

namespace researchProject.Models.ViewModels
{
    public class AuditLogViewModel
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? UserName { get; set; }
        public string? UserRole { get; set; }
        public string ControllerName { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public int StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public string? Description { get; set; }
        public string? ExceptionDetails { get; set; }
        public long ExecutionDurationMs { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

