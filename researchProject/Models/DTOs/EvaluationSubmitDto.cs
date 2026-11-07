using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.DTOs
{
    public class EvaluationSubmitDto
    {
        [Required]
        public int EvaluationId { get; set; }

        [Required(ErrorMessage = "تقييم معيار حماية الخصوصية إلزامي")]
        [Display(Name = "معيار حماية الخصوصية وسرية البيانات")]
        public string PrivacyProtectionRating { get; set; } = string.Empty;

        [Required(ErrorMessage = "تقييم معيار نسبة المخاطر إلزامي")]
        [Display(Name = "معيار تقييم نسبة المخاطر والموازنة مع الفوائد")]
        public string RiskLevel { get; set; } = string.Empty;

        [Required(ErrorMessage = "تقييم معيار الموافقة المستنيرة إلزامي")]
        [Display(Name = "معيار إجراءات الموافقة المستنيرة")]
        public string InformedConsentRating { get; set; } = string.Empty;

        [Required(ErrorMessage = "قرار التقييم إلزامي")]
        [Display(Name = "قرار التحكيم النهائي للجولة")]
        public string Decision { get; set; } = string.Empty;

        [Display(Name = "توصيات وملاحظات اللجنة")]
        public string? Recommendations { get; set; }

        [Display(Name = "الملاحظات والتعديلات المطلوبة من الباحث")]
        public string? ModificationNotes { get; set; }
    }
}
