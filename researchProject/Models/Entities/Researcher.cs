using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.Entities
{
    public class Researcher
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "معرف المستخدم مطلوب")]
        [Display(Name = "معرف المستخدم")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "الكلية مطلوبة")]
        [Display(Name = "الكلية")]
        public int CollegeId { get; set; }

        [Required(ErrorMessage = "فئة الباحث مطلوبة")]
        [Display(Name = "فئة الباحث")]
        [StringLength(50, ErrorMessage = "فئة الباحث يجب ألا تتجاوز 50 حرفاً")]
        public string ResearcherType { get; set; } = string.Empty;

        [Required(ErrorMessage = "الدرجة العلمية مطلوبة")]
        [Display(Name = "الدرجة العلمية")]
        [StringLength(100, ErrorMessage = "الدرجة العلمية يجب ألا تتجاوز 100 حرف")]
        public string AcademicDegree { get; set; } = string.Empty;

        [Required(ErrorMessage = "القسم الأكاديمي مطلوب")]
        [Display(Name = "القسم الأكاديمي")]
        [StringLength(150, ErrorMessage = "القسم يجب ألا يتجاوز 150 حرفاً")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "الرقم القومي مطلوب")]
        [Display(Name = "الرقم القومي")]
        [StringLength(14, MinimumLength = 14, ErrorMessage = "الرقم القومي يجب أن يتكون من 14 رقماً")]
        public string NationalId { get; set; } = string.Empty;

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual College College { get; set; } = null!;
        public virtual ICollection<ResearcherResearchesBridge> ResearcherResearchesBridges { get; set; } = new List<ResearcherResearchesBridge>();
    }
}
