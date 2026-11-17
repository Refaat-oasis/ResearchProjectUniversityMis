using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.DTOs
{
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [Display(Name = "الاسم الكامل")]
        [StringLength(150, ErrorMessage = "الاسم الكامل يجب ألا يتجاوز 150 حرفاً")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Display(Name = "رقم الهاتف")]
        [Phone(ErrorMessage = "صيغة رقم الهاتف غير صالحة")]
        [RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "رقم الهاتف يجب أن يكون رقم محمول مصري صحيح مكون من 11 رقماً يبدأ بـ 01")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "الدرجة العلمية")]
        [StringLength(100, ErrorMessage = "الدرجة العلمية يجب ألا تتجاوز 100 حرف")]
        public string? AcademicDegree { get; set; }

        [Display(Name = "فئة الباحث")]
        [StringLength(50, ErrorMessage = "فئة الباحث يجب ألا تتجاوز 50 حرفاً")]
        public string? ResearcherType { get; set; }
    }
}

