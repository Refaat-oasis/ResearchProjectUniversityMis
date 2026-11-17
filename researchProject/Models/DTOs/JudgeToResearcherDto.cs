using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.DTOs
{
    public class JudgeToResearcherDto
    {
        [Required(ErrorMessage = "يرجى اختيار الكلية")]
        [Display(Name = "الكلية")]
        public int CollegeId { get; set; }

        [Required(ErrorMessage = "القسم الأكاديمي مطلوب")]
        [Display(Name = "القسم")]
        [StringLength(100, ErrorMessage = "اسم القسم يجب ألا يتجاوز 100 حرف")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "فئة الباحث مطلوبة")]
        [Display(Name = "فئة الباحث")]
        [StringLength(50)]
        public string ResearcherType { get; set; } = string.Empty;

        [Required(ErrorMessage = "الدرجة العلمية مطلوبة")]
        [Display(Name = "الدرجة العلمية")]
        [StringLength(100)]
        public string AcademicDegree { get; set; } = string.Empty;

        [Required(ErrorMessage = "الرقم القومي مطلوب")]
        [Display(Name = "الرقم القومي")]
        [RegularExpression(@"^[0-9]{14}$", ErrorMessage = "الرقم القومي يجب أن يتكون من 14 رقماً")]
        public string NationalId { get; set; } = string.Empty;
    }
}

