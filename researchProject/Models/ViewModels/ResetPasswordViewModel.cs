using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "رمز التحقق والاستعادة")]
        public string Token { get; set; } = string.Empty;

        [Display(Name = "كلمة المرور الجديدة")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Display(Name = "تأكيد كلمة المرور الجديدة")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
    }
}
