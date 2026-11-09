using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.DTOs
{
    public class CollegeUpdateDto
    {
        [Required(ErrorMessage = "معرف الكلية مطلوب")]
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الكلية مطلوب")]
        [StringLength(100, ErrorMessage = "اسم الكلية يجب ألا يتجاوز 100 حرف")]
        [Display(Name = "اسم الكلية")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "رمز الكلية مطلوب")]
        [StringLength(20, ErrorMessage = "رمز الكلية يجب ألا يتجاوز 20 حرفاً")]
        [Display(Name = "رمز الكلية الكودي")]
        public string Code { get; set; } = string.Empty;
    }
}
