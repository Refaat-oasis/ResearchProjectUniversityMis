using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.ViewModels
{
    public class AcceptInvitationViewModel
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Specialty { get; set; }

        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "كلمة المرور")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "تأكيد كلمة المرور")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "اللقب الأكاديمي (أستاذ / أستاذ مشارك / خبير)")]
        public string AcademicTitle { get; set; } = string.Empty;

        public bool IsExistingUser { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
