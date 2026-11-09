using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.ViewModels
{
    public class LoginViewModel
    {
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "كلمة المرور")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "تذكرني على هذا الجهاز")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
