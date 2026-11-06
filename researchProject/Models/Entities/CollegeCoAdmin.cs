using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.Entities
{
    public class CollegeCoAdmin
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "معرف المستخدم مطلوب")]
        [Display(Name = "معرف المستخدم")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "الكلية مطلوبة")]
        [Display(Name = "الكلية")]
        public int CollegeId { get; set; }

        [Display(Name = "مسار صورة التوقيع")]
        [StringLength(500, ErrorMessage = "مسار التوقيع يجب ألا يتجاوز 500 حرف")]
        public string? SignatureImagePath { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual College College { get; set; } = null!;
    }
}
