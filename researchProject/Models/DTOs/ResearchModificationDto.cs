using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace researchProject.Models.DTOs
{
    public class ResearchModificationDto
    {
        [Required]
        public int ResearchId { get; set; }

        [Display(Name = "ملاحظات وتوضيحات الباحث حول التعديلات")]
        public string? ResearcherNotes { get; set; }

        [Required(ErrorMessage = "يجب رفع ملف المقترح المعدل")]
        [Display(Name = "ملف المقترح البحثي بعد التعديل")]
        public IFormFile ModifiedProposalFile { get; set; } = null!;

        [Display(Name = "ملف المرفقات أو الأدوات المعدلة")]
        public IFormFile? ModifiedToolsFile { get; set; }
    }
}

