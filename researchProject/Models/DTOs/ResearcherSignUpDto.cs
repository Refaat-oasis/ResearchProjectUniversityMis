using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.DTOs
{
    public class ResearcherSignUpDto
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [Display(Name = "الاسم الكامل (رباعي)")]
        [StringLength(150, ErrorMessage = "الاسم يجب ألا يتجاوز 150 حرفاً")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني المؤسسي مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صالحة")]
        [Display(Name = "البريد الإلكتروني الأكاديمي (المؤسسي)")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور يجب أن لا تقل عن 6 أحرف")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "كلمتا المرور غير متطابقتين")]
        [Display(Name = "تأكيد كلمة المرور")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "الكلية مطلوبة")]
        [Display(Name = "الكلية التابع لها")]
        public int CollegeId { get; set; }

        [Required(ErrorMessage = "فئة الباحث مطلوبة")]
        [Display(Name = "فئة الباحث")]
        public string ResearcherType { get; set; } = string.Empty;

        [Required(ErrorMessage = "الدرجة العلمية مطلوبة")]
        [Display(Name = "الدرجة العلمية")]
        public string AcademicDegree { get; set; } = string.Empty;

        [Required(ErrorMessage = "القسم الأكاديمي مطلوب")]
        [Display(Name = "القسم الأكاديمي")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "الرقم القومي مطلوب")]
        [StringLength(14, MinimumLength = 14, ErrorMessage = "الرقم القومي يجب أن يتكون من 14 رقماً")]
        [Display(Name = "الرقم القومي (14 رقم)")]
        public string NationalId { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الهاتف المحمول مطلوب")]
        [Phone(ErrorMessage = "رقم الهاتف غير صالح")]
        [Display(Name = "رقم الهاتف المحمول")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
