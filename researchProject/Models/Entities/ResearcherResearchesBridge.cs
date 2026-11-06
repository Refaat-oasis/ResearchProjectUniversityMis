using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.Entities
{
    public class ResearcherResearchesBridge
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "معرف البحث مطلوب")]
        [Display(Name = "معرف البحث")]
        public int ResearchId { get; set; }

        [Required(ErrorMessage = "معرف الباحث مطلوب")]
        [Display(Name = "معرف الباحث")]
        public int ResearcherId { get; set; }

        [Display(Name = "الباحث الرئيسي")]
        public bool IsMainAuthor { get; set; }

        // Navigation properties
        public virtual Research Research { get; set; } = null!;
        public virtual Researcher Researcher { get; set; } = null!;
    }
}
