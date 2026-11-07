using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.Entities
{
    public class Department
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم القسم الأكاديمي مطلوب")]
        [Display(Name = "اسم القسم الأكاديمي")]
        [StringLength(150, ErrorMessage = "اسم القسم يجب ألا يتجاوز 150 حرفاً")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "الكلية مطلوبة")]
        public int CollegeId { get; set; }

        public virtual College College { get; set; } = null!;
    }
}
