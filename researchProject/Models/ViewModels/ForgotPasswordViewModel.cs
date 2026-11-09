using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
