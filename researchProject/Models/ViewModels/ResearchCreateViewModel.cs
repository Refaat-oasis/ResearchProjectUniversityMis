using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using researchProject.Models.Entities;

namespace researchProject.Models.ViewModels
{
    public class ResearchCreateViewModel
    {
        [Display(Name = "عنوان المقترح البحثي")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "الكلية")]
        public int CollegeId { get; set; }
        public string CollegeName { get; set; } = string.Empty;

        [Display(Name = "تصنيف البحث")]
        public int CategoryId { get; set; }

        [Display(Name = "التخصص الأكاديمي الدقيق")]
        public string AcademicSpecialty { get; set; } = string.Empty;

        public List<ResearchCategory> AvailableCategories { get; set; } = new List<ResearchCategory>();
        public Dictionary<string, string> DynamicAnswers { get; set; } = new Dictionary<string, string>();
        public string? ErrorMessage { get; set; }
    }
}
