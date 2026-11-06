using System;
using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.Entities
{
    public class JudgeInvitation
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [Display(Name = "البريد الإلكتروني")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صالح")]
        [StringLength(256, ErrorMessage = "البريد يجب ألا يتجاوز 256 حرفاً")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "تجزئة الرمز مطلوبة")]
        [Display(Name = "تجزئة الرمز الأمني")]
        [StringLength(128, ErrorMessage = "تجزئة الرمز يجب ألا تتجاوز 128 حرفاً")]
        public string TokenHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "معرف الداعي مطلوب")]
        [Display(Name = "معرف الداعي")]
        public string InvitedByUserId { get; set; } = string.Empty;

        [Display(Name = "التخصص الأكاديمي")]
        [StringLength(150, ErrorMessage = "التخصص يجب ألا يتجاوز 150 حرفاً")]
        public string? Specialty { get; set; }

        [Display(Name = "تاريخ الإرسال")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "تاريخ انتهاء الصلاحية")]
        public DateTime ExpiresAt { get; set; }

        [Display(Name = "تاريخ القبول")]
        public DateTime? AcceptedAt { get; set; }

        [Required(ErrorMessage = "حالة الدعوة مطلوبة")]
        [Display(Name = "حالة الدعوة")]
        [StringLength(50, ErrorMessage = "حالة الدعوة يجب ألا تتجاوز 50 حرفاً")]
        public string Status { get; set; } = string.Empty;

        // Navigation properties
        public virtual ApplicationUser InvitedByUser { get; set; } = null!;
    }
}
