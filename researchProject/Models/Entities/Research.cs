using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.Entities
{
    public class Research
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان البحث مطلوب")]
        [Display(Name = "عنوان المقترح البحثي")]
        [StringLength(300, ErrorMessage = "عنوان البحث يجب ألا يتجاوز 300 حرف")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "الكلية مطلوبة")]
        [Display(Name = "الكلية")]
        public int CollegeId { get; set; }

        [Required(ErrorMessage = "تصنيف البحث مطلوب")]
        [Display(Name = "تصنيف البحث")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "التخصص الأكاديمي الدقيق مطلوب")]
        [Display(Name = "التخصص الأكاديمي الدقيق")]
        [StringLength(150, ErrorMessage = "التخصص يجب ألا يتجاوز 150 حرفاً")]
        public string AcademicSpecialty { get; set; } = string.Empty;

        [Display(Name = "إجابات الحقول الديناميكية")]
        public string? DynamicFieldsJson { get; set; }

        [Required(ErrorMessage = "حالة سير العمل مطلوبة")]
        [Display(Name = "حالة سير العمل")]
        [StringLength(50, ErrorMessage = "حالة البحث يجب ألا تتجاوز 50 حرفاً")]
        public string WorkflowStatus { get; set; } = string.Empty;

        [Display(Name = "تاريخ ووقت التقديم")]
        public DateTime SubmittedAt { get; set; }

        [Display(Name = "الموعد النهائي للفحص الأولي")]
        public DateTime ScreeningDueAt { get; set; }

        [Display(Name = "ملاحظات الفحص الشكلي")]
        public string? ScreeningNotes { get; set; }

        [Display(Name = "الرقم المسلسل للشهادة")]
        [StringLength(100, ErrorMessage = "رقم الشهادة يجب ألا يتجاوز 100 حرف")]
        public string? CertificateSerialNumber { get; set; }

        [Display(Name = "رابط التحقق السريع (QR)")]
        [StringLength(500, ErrorMessage = "رابط التحقق يجب ألا يتجاوز 500 حرف")]
        public string? CertificateQrCodeUrl { get; set; }

        [Display(Name = "تاريخ إصدار الشهادة")]
        public DateTime? CertificateIssuedAt { get; set; }

        [Display(Name = "مسار ملف الشهادة (PDF)")]
        [StringLength(500, ErrorMessage = "مسار الشهادة يجب ألا يتجاوز 500 حرف")]
        public string? CertificateFilePath { get; set; }

        // Navigation properties
        public virtual College College { get; set; } = null!;
        public virtual ResearchCategory Category { get; set; } = null!;
        public virtual ICollection<ResearcherResearchesBridge> ResearcherResearchesBridges { get; set; } = new List<ResearcherResearchesBridge>();
        public virtual ICollection<ResearchAttachment> ResearchAttachments { get; set; } = new List<ResearchAttachment>();
        public virtual ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
        public virtual ICollection<ResearchStatusHistory> ResearchStatusHistories { get; set; } = new List<ResearchStatusHistory>();
    }
}
