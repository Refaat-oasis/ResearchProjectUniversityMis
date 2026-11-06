using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.Entities
{
    public class ResearchCategory
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الكلية مطلوبة")]
        [Display(Name = "الكلية")]
        public int CollegeId { get; set; }

        [Required(ErrorMessage = "اسم التصنيف مطلوب")]
        [Display(Name = "اسم التصنيف")]
        [StringLength(150, ErrorMessage = "اسم التصنيف يجب ألا يتجاوز 150 حرفاً")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "وصف التصنيف")]
        [StringLength(500, ErrorMessage = "الوصف يجب ألا يتجاوز 500 حرف")]
        public string? Description { get; set; }

        [Display(Name = "تعريف الحقول الإضافية")]
        public string? FieldsDefinitionJson { get; set; }

        // Navigation properties
        public virtual College College { get; set; } = null!;
        public virtual ICollection<Research> Researches { get; set; } = new List<Research>();
    }
}
