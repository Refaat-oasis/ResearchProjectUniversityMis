using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace researchProject.Models.DTOs
{
    public class ResearchCreateDto
    {
        [Required(ErrorMessage = "عنوان البحث مطلوب")]
        [Display(Name = "عنوان المقترح البحثي")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "الكلية مطلوبة")]
        [Display(Name = "الكلية")]
        public int CollegeId { get; set; }

        [Required(ErrorMessage = "نوع وتصنيف البحث مطلوب")]
        [Display(Name = "تصنيف البحث")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "التخصص الأكاديمي مطلوب")]
        [Display(Name = "التخصص الأكاديمي الدقيق")]
        public string AcademicSpecialty { get; set; } = string.Empty;

        // Dynamic fields answers stored as JSON key-value
        public Dictionary<string, string> DynamicAnswers { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> DynamicFields { get => DynamicAnswers; set => DynamicAnswers = value; }

        // Mandatory attachments
        [Required(ErrorMessage = "ملف وثيقة الموافقة المستنيرة إلزامي")]
        [Display(Name = "وثيقة الموافقة المستنيرة (Informed Consent)")]
        public IFormFile InformedConsentFile { get; set; } = null!;

        [Required(ErrorMessage = "ملف أدوات البحث إلزامي")]
        [Display(Name = "أدوات البحث (استبيانات / استمارات / أدلة)")]
        public IFormFile ResearchToolsFile { get; set; } = null!;

        [Required(ErrorMessage = "ملف خطة البحث والمقترح إلزامي")]
        [Display(Name = "خطة المقترح البحثي الكاملة (Research Proposal)")]
        public IFormFile ResearchProposalFile { get; set; } = null!;

        [Display(Name = "مستندات إضافية أو ملاحق")]
        public IFormFile? AdditionalDocumentFile { get; set; }
    }
}

