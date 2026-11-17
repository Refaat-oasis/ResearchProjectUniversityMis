using System;
using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Display(Name = "معرف المستخدم")]
        [StringLength(450)]
        public string? UserId { get; set; }

        [Display(Name = "البريد الإلكتروني")]
        [StringLength(256)]
        public string? UserEmail { get; set; }

        [Display(Name = "اسم المستخدم")]
        [StringLength(256)]
        public string? UserName { get; set; }

        [Display(Name = "الدور")]
        [StringLength(100)]
        public string? UserRole { get; set; }

        [Required]
        [Display(Name = "وحدة التحكم")]
        [StringLength(100)]
        public string ControllerName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "العملية")]
        [StringLength(100)]
        public string ActionName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "طريقة الطلب")]
        [StringLength(10)]
        public string HttpMethod { get; set; } = string.Empty;

        [Required]
        [Display(Name = "المسار")]
        [StringLength(1000)]
        public string Path { get; set; } = string.Empty;

        [Display(Name = "عنوان IP")]
        [StringLength(50)]
        public string? IpAddress { get; set; }

        [Display(Name = "رمز الاستجابة")]
        public int StatusCode { get; set; }

        [Display(Name = "الحالة")]
        public bool IsSuccess { get; set; }

        [Display(Name = "تفاصيل العملية")]
        public string? Description { get; set; }

        [Display(Name = "تفاصيل الاستثناء")]
        public string? ExceptionDetails { get; set; }

        [Display(Name = "مدة التنفيذ (مللي ثانية)")]
        public long ExecutionDurationMs { get; set; }

        [Display(Name = "وقت وتاريخ العملية")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

