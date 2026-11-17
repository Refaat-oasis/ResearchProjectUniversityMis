using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.Entities
{
    public class Judge
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "معرف المستخدم مطلوب")]
        [Display(Name = "معرف المستخدم")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "التخصص الأكاديمي مطلوب")]
        [Display(Name = "التخصص الأكاديمي")]
        [StringLength(150, ErrorMessage = "التخصص يجب ألا يتجاوز 150 حرفاً")]
        public string Specialty { get; set; } = string.Empty;

        [Required(ErrorMessage = "اللقب الأكاديمي مطلوب")]
        [Display(Name = "اللقب الأكاديمي")]
        [StringLength(100, ErrorMessage = "اللقب الأكاديمي يجب ألا يتجاوز 100 حرف")]
        public string AcademicTitle { get; set; } = string.Empty;

        [Display(Name = "فئة الباحث")]
        [StringLength(50, ErrorMessage = "فئة الباحث يجب ألا تتجاوز 50 حرفاً")]
        public string? ResearcherType { get; set; }

        [Display(Name = "الكلية التابع لها")]
        public int? CollegeId { get; set; }

        [Display(Name = "نشط")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual College? College { get; set; }
        public virtual ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
    }
}
