using System;
using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.Entities
{
    public class EmailVerificationCode
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [Display(Name = "البريد الإلكتروني")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صالح")]
        [StringLength(256, ErrorMessage = "البريد يجب ألا يتجاوز 256 حرفاً")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "معرف المستخدم")]
        [StringLength(450)]
        public string? UserId { get; set; }

        [Required(ErrorMessage = "رمز التحقق مطلوب")]
        [Display(Name = "رمز التحقق")]
        [StringLength(10, MinimumLength = 6, ErrorMessage = "رمز التحقق يجب أن يكون 6 أرقام")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "الغرض من الرمز مطلوب")]
        [Display(Name = "الغرض")]
        [StringLength(50)]
        public string Purpose { get; set; } = "EmailConfirmation"; // "EmailConfirmation", "PasswordReset"

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "تاريخ انتهاء الصلاحية")]
        public DateTime ExpiresAt { get; set; } // Strictly CreatedAt + 15 Minutes

        [Display(Name = "تم الاستخدام")]
        public bool IsUsed { get; set; } = false;

        [Display(Name = "تاريخ الاستخدام")]
        public DateTime? UsedAt { get; set; }

        [Display(Name = "عدد المحاولات الخاطئة")]
        public int AttemptsCount { get; set; } = 0; // Max 5 attempts
    }
}

