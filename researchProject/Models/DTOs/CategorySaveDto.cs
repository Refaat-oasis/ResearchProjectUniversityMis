using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.DTOs
{
    public class CategorySaveDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الكلية مطلوبة")]
        [Display(Name = "الكلية التابع لها")]
        public int CollegeId { get; set; }

        [Required(ErrorMessage = "اسم التصنيف مطلوب")]
        [StringLength(150, ErrorMessage = "اسم التصنيف يجب ألا يتجاوز 150 حرفاً")]
        [Display(Name = "اسم تصنيف البحث")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "الوصف يجب ألا يتجاوز 500 حرف")]
        [Display(Name = "وصف تصنيف البحث")]
        public string? Description { get; set; }

        [Display(Name = "تعريف الحقول الديناميكية بصيغة JSON")]
        public string? FieldsDefinitionJson { get; set; }

        [Display(Name = "حالة التفعيل")]
        public bool IsActive { get; set; } = true;
    }
}
