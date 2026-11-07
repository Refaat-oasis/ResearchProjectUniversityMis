using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.DTOs
{
    public class VerifyEmailDto
    {
        [Required(ErrorMessage = "معرف الحساب أو البريد الإلكتروني مطلوب")]
        [Display(Name = "البريد الإلكتروني أو المعرف")]
        public string EmailOrUserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "رمز التحقق مطلوب")]
        [Display(Name = "رمز التحقق")]
        public string Token { get; set; } = string.Empty;
    }
}
