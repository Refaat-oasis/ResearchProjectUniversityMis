using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using researchProject.Models.Entities;

namespace researchProject.Models.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "تذكرني على هذا الجهاز")]
        public bool RememberMe { get; set; }
    }

    public class ResearcherSignUpDto
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [Display(Name = "الاسم الكامل (رباعي)")]
        [StringLength(150, ErrorMessage = "الاسم يجب ألا يتجاوز 150 حرفاً")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني المؤسسي مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صالحة")]
        [Display(Name = "البريد الإلكتروني الأكاديمي (المؤسسي)")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور يجب أن لا تقل عن 6 أحرف")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "كلمتا المرور غير متطابقتين")]
        [Display(Name = "تأكيد كلمة المرور")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "الكلية مطلوبة")]
        [Display(Name = "الكلية التابع لها")]
        public int CollegeId { get; set; }

        [Required(ErrorMessage = "فئة الباحث مطلوبة")]
        [Display(Name = "فئة الباحث")]
        public string ResearcherType { get; set; } = string.Empty;

        [Required(ErrorMessage = "الدرجة العلمية مطلوبة")]
        [Display(Name = "الدرجة العلمية")]
        public string AcademicDegree { get; set; } = string.Empty;

        [Required(ErrorMessage = "القسم الأكاديمي مطلوب")]
        [Display(Name = "القسم الأكاديمي")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "الرقم القومي مطلوب")]
        [StringLength(14, MinimumLength = 14, ErrorMessage = "الرقم القومي يجب أن يتكون من 14 رقماً")]
        [Display(Name = "الرقم القومي (14 رقم)")]
        public string NationalId { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الهاتف المحمول مطلوب")]
        [Phone(ErrorMessage = "رقم الهاتف غير صالح")]
        [Display(Name = "رقم الهاتف المحمول")]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class AcceptInvitationDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور يجب ألا تقل عن 6 أحرف")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "كلمتا المرور غير متطابقتين")]
        [Display(Name = "تأكيد كلمة المرور")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "اللقب الأكاديمي مطلوب")]
        [Display(Name = "اللقب الأكاديمي (أستاذ / أستاذ مشارك / خبير)")]
        public string AcademicTitle { get; set; } = string.Empty;
    }

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

    public class CertificateVerificationResultDto
    {
        public bool IsValid { get; set; }
        public string? SerialNumber { get; set; }
        public string? ResearchTitle { get; set; }
        public string? CollegeName { get; set; }
        public DateTime? IssueDate { get; set; }
        public string? Message { get; set; }
    }

    public class SystemStatisticsDto
    {
        public int TotalResearches { get; set; }
        public int UnderScreeningCount { get; set; }
        public int AssignedToJudgesCount { get; set; }
        public int ModificationsRequiredCount { get; set; }
        public int ApprovedCount { get; set; }
        public int CertificatesIssuedCount { get; set; }
        public int TotalColleges { get; set; }
        public int TotalJudges { get; set; }
        public int TotalResearchers { get; set; }
        public double AverageScreeningHours { get; set; }
        public double AverageJudgingDays { get; set; }
        public double SlaCompliancePercentage { get; set; }
        public Dictionary<string, int> ResearchesByCollege { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ResearchesByCategory { get; set; } = new Dictionary<string, int>();
    }
}
