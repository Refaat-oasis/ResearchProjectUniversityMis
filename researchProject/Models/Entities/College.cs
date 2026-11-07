using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.Entities
{
    public class College
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الكلية مطلوب")]
        [Display(Name = "اسم الكلية")]
        [StringLength(150, ErrorMessage = "اسم الكلية يجب ألا يتجاوز 150 حرفاً")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "رمز الكلية مطلوب")]
        [Display(Name = "رمز الكلية")]
        [StringLength(20, ErrorMessage = "رمز الكلية يجب ألا يتجاوز 20 حرفاً")]
        public string Code { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<CollegeCoAdmin> CollegeCoAdmins { get; set; } = new List<CollegeCoAdmin>();
        public virtual ICollection<Researcher> Researchers { get; set; } = new List<Researcher>();
        public virtual ICollection<ResearchCategory> ResearchCategories { get; set; } = new List<ResearchCategory>();
        public virtual ICollection<Research> Researches { get; set; } = new List<Research>();
        public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
    }
}
