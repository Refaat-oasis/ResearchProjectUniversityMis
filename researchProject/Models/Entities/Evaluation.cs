using System;
using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.Entities
{
    public class Evaluation
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "معرف البحث مطلوب")]
        [Display(Name = "معرف البحث")]
        public int ResearchId { get; set; }

        [Required(ErrorMessage = "معرف المحكم مطلوب")]
        [Display(Name = "معرف المحكم")]
        public int JudgeId { get; set; }

        [Required(ErrorMessage = "رقم الجولة مطلوب")]
        [Display(Name = "رقم الجولة")]
        public int RoundNumber { get; set; } = 1;

        [Required(ErrorMessage = "قرار التقييم مطلوب")]
        [Display(Name = "قرار التقييم")]
        [StringLength(50, ErrorMessage = "القرار يجب ألا يتجاوز 50 حرفاً")]
        public string Decision { get; set; } = string.Empty;

        [Display(Name = "تقييم حماية الخصوصية")]
        [StringLength(50, ErrorMessage = "تقييم الخصوصية يجب ألا يتجاوز 50 حرفاً")]
        public string? PrivacyProtectionRating { get; set; }

        [Display(Name = "تقييم نسبة المخاطر")]
        [StringLength(50, ErrorMessage = "تقييم المخاطر يجب ألا يتجاوز 50 حرفاً")]
        public string? RiskLevel { get; set; }

        [Display(Name = "تقييم الموافقة المستنيرة")]
        [StringLength(50, ErrorMessage = "تقييم الموافقة المستنيرة يجب ألا يتجاوز 50 حرفاً")]
        public string? InformedConsentRating { get; set; }

        [Display(Name = "التوصيات الإلزامية")]
        public string? Recommendations { get; set; }

        [Display(Name = "ملاحظات التعديل المطلوبة")]
        public string? ModificationNotes { get; set; }

        [Display(Name = "تم التوقيع رقمياً")]
        public bool IsDigitallySigned { get; set; }

        [Display(Name = "تاريخ التوقيع الرقمي")]
        public DateTime? SignedAt { get; set; }

        [Display(Name = "تاريخ تقديم التقييم")]
        public DateTime? SubmittedAt { get; set; }

        // Navigation properties
        public virtual Research Research { get; set; } = null!;
        public virtual Judge Judge { get; set; } = null!;
    }
}
