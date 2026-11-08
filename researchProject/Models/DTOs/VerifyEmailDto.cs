using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.DTOs
{
    public class VerifyEmailDto
    {
        [Required(ErrorMessage = "البريد الإلكتروني أو معرف الحساب مطلوب")]
        [Display(Name = "البريد الإلكتروني")]
        public string EmailOrUserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "رمز التحقق مطلوب")]
        [Display(Name = "رمز التحقق الرقمي (6 أرقام)")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "رمز التحقق يجب أن يتكون من 6 أرقام.")]
        public string Token { get; set; } = string.Empty;
    }
}
