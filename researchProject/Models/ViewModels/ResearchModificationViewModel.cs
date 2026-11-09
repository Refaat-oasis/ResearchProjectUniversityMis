using System.ComponentModel.DataAnnotations;
using researchProject.Models.Entities;

namespace researchProject.Models.ViewModels
{
    public class ResearchModificationViewModel
    {
        public int ResearchId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int CurrentRound { get; set; } = 1;
        public string? ModificationNotes { get; set; }

        [Display(Name = "ملاحظات وتوضيحات الباحث حول التعديلات")]
        public string? ResearcherNotes { get; set; }

        public Research? Research { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
