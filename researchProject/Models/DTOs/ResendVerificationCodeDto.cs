using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.DTOs
{
    public class ResendVerificationCodeDto
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [Display(Name = "البريد الإلكتروني")]
        public string EmailOrUserId { get; set; } = string.Empty;
    }
}

