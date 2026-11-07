using System;

namespace researchProject.Models.Enums
{
    public static class EnumExtensions
    {
        public static string ToArabicString(this ResearchStatus status)
        {
            return status switch
            {
                ResearchStatus.UnderFormalScreening => "قيد المراجعة الشكلية",
                ResearchStatus.AssignedToJudges => "مسند للمحكمين",
                ResearchStatus.ModificationsRequired => "تعديلات مطلوبة",
                ResearchStatus.Approved => "معتمد",
                ResearchStatus.CertificateIssued => "صادر الشهادة",
                _ => status.ToString()
            };
        }

        public static string ToArabicString(this UserRole role)
        {
            return role switch
            {
                UserRole.SuperAdmin => "مدير النظام العام",
                UserRole.CollegeCoAdmin => "مدير اللجنة الفرعية بالكلية",
                UserRole.Judge => "محكم علمي",
                UserRole.Researcher => "باحث",
                _ => role.ToString()
            };
        }

        public static string ToArabicString(this InvitationStatus status)
        {
            return status switch
            {
                InvitationStatus.Pending => "معلق",
                InvitationStatus.Accepted => "مقبول",
                InvitationStatus.Expired => "منتهي",
                InvitationStatus.Revoked => "ملغى",
                _ => status.ToString()
            };
        }

        public static string ToArabicString(this ResearcherType type)
        {
            return type switch
            {
                ResearcherType.FacultyMember => "عضو هيئة تدريس",
                ResearcherType.Researcher => "باحث",
                ResearcherType.PostgraduateStudent => "طالب دراسات عليا",
                _ => type.ToString()
            };
        }

        public static string ToArabicString(this AttachmentType type)
        {
            return type switch
            {
                AttachmentType.InformedConsent => "الموافقة المستنيرة",
                AttachmentType.ResearchTools => "أدوات البحث",
                AttachmentType.ResearchProposal => "خطة البحث",
                AttachmentType.AdditionalDocument => "مستند إضافي",
                AttachmentType.ModifiedFile => "ملف معدل",
                _ => type.ToString()
            };
        }

        public static string ToArabicString(this EvaluationDecision decision)
        {
            return decision switch
            {
                EvaluationDecision.UnderEvaluation => "قيد التقييم",
                EvaluationDecision.Approved => "موافق",
                EvaluationDecision.ModificationsRequired => "طلب تعديلات",
                EvaluationDecision.ClosedDueToOtherJudge => "مغلق لاكتمال التحكيم",
                _ => decision.ToString()
            };
        }

        public static string ToArabicString(this EthicsRating rating)
        {
            return rating switch
            {
                EthicsRating.Appropriate => "مناسب",
                EthicsRating.NeedsModification => "يحتاج تعديل",
                EthicsRating.Inappropriate => "غير مناسب",
                _ => rating.ToString()
            };
        }

        public static ResearchStatus ToResearchStatus(this string? status)
        {
            return status?.Trim() switch
            {
                "قيد المراجعة الشكلية" => ResearchStatus.UnderFormalScreening,
                "مسند للمحكمين" => ResearchStatus.AssignedToJudges,
                "تعديلات مطلوبة" => ResearchStatus.ModificationsRequired,
                "معتمد" => ResearchStatus.Approved,
                "صادر الشهادة" => ResearchStatus.CertificateIssued,
                nameof(ResearchStatus.UnderFormalScreening) => ResearchStatus.UnderFormalScreening,
                nameof(ResearchStatus.AssignedToJudges) => ResearchStatus.AssignedToJudges,
                nameof(ResearchStatus.ModificationsRequired) => ResearchStatus.ModificationsRequired,
                nameof(ResearchStatus.Approved) => ResearchStatus.Approved,
                nameof(ResearchStatus.CertificateIssued) => ResearchStatus.CertificateIssued,
                _ => Enum.TryParse<ResearchStatus>(status, true, out var parsed) ? parsed : ResearchStatus.UnderFormalScreening
            };
        }

        public static UserRole ToUserRole(this string? role)
        {
            return role?.Trim() switch
            {
                "مدير النظام العام" => UserRole.SuperAdmin,
                "مدير اللجنة الفرعية بالكلية" => UserRole.CollegeCoAdmin,
                "محكم علمي" => UserRole.Judge,
                "باحث" => UserRole.Researcher,
                nameof(UserRole.SuperAdmin) => UserRole.SuperAdmin,
                nameof(UserRole.CollegeCoAdmin) => UserRole.CollegeCoAdmin,
                nameof(UserRole.Judge) => UserRole.Judge,
                nameof(UserRole.Researcher) => UserRole.Researcher,
                _ => Enum.TryParse<UserRole>(role, true, out var parsed) ? parsed : UserRole.Researcher
            };
        }

        public static InvitationStatus ToInvitationStatus(this string? status)
        {
            return status?.Trim() switch
            {
                "معلق" => InvitationStatus.Pending,
                "مقبول" => InvitationStatus.Accepted,
                "منتهي" => InvitationStatus.Expired,
                "ملغى" => InvitationStatus.Revoked,
                nameof(InvitationStatus.Pending) => InvitationStatus.Pending,
                nameof(InvitationStatus.Accepted) => InvitationStatus.Accepted,
                nameof(InvitationStatus.Expired) => InvitationStatus.Expired,
                nameof(InvitationStatus.Revoked) => InvitationStatus.Revoked,
                _ => Enum.TryParse<InvitationStatus>(status, true, out var parsed) ? parsed : InvitationStatus.Pending
            };
        }

        public static ResearcherType ToResearcherType(this string? type)
        {
            return type?.Trim() switch
            {
                "عضو هيئة تدريس" => ResearcherType.FacultyMember,
                "باحث" => ResearcherType.Researcher,
                "طالب دراسات عليا" => ResearcherType.PostgraduateStudent,
                nameof(ResearcherType.FacultyMember) => ResearcherType.FacultyMember,
                nameof(ResearcherType.Researcher) => ResearcherType.Researcher,
                nameof(ResearcherType.PostgraduateStudent) => ResearcherType.PostgraduateStudent,
                _ => Enum.TryParse<ResearcherType>(type, true, out var parsed) ? parsed : ResearcherType.Researcher
            };
        }

        public static AttachmentType ToAttachmentType(this string? type)
        {
            return type?.Trim() switch
            {
                "الموافقة المستنيرة" => AttachmentType.InformedConsent,
                "أدوات البحث" => AttachmentType.ResearchTools,
                "خطة البحث" => AttachmentType.ResearchProposal,
                "مستند إضافي" => AttachmentType.AdditionalDocument,
                "ملف معدل" => AttachmentType.ModifiedFile,
                nameof(AttachmentType.InformedConsent) => AttachmentType.InformedConsent,
                nameof(AttachmentType.ResearchTools) => AttachmentType.ResearchTools,
                nameof(AttachmentType.ResearchProposal) => AttachmentType.ResearchProposal,
                nameof(AttachmentType.AdditionalDocument) => AttachmentType.AdditionalDocument,
                nameof(AttachmentType.ModifiedFile) => AttachmentType.ModifiedFile,
                _ => Enum.TryParse<AttachmentType>(type, true, out var parsed) ? parsed : AttachmentType.AdditionalDocument
            };
        }

        public static EvaluationDecision ToEvaluationDecision(this string? decision)
        {
            return decision?.Trim() switch
            {
                "قيد التقييم" => EvaluationDecision.UnderEvaluation,
                "موافق" => EvaluationDecision.Approved,
                "طلب تعديلات" => EvaluationDecision.ModificationsRequired,
                "مغلق لاكتمال التحكيم" => EvaluationDecision.ClosedDueToOtherJudge,
                nameof(EvaluationDecision.UnderEvaluation) => EvaluationDecision.UnderEvaluation,
                nameof(EvaluationDecision.Approved) => EvaluationDecision.Approved,
                nameof(EvaluationDecision.ModificationsRequired) => EvaluationDecision.ModificationsRequired,
                nameof(EvaluationDecision.ClosedDueToOtherJudge) => EvaluationDecision.ClosedDueToOtherJudge,
                _ => Enum.TryParse<EvaluationDecision>(decision, true, out var parsed) ? parsed : EvaluationDecision.UnderEvaluation
            };
        }

        public static EthicsRating ToEthicsRating(this string? rating)
        {
            return rating?.Trim() switch
            {
                "مناسب" => EthicsRating.Appropriate,
                "يحتاج تعديل" => EthicsRating.NeedsModification,
                "غير مناسب" => EthicsRating.Inappropriate,
                nameof(EthicsRating.Appropriate) => EthicsRating.Appropriate,
                nameof(EthicsRating.NeedsModification) => EthicsRating.NeedsModification,
                nameof(EthicsRating.Inappropriate) => EthicsRating.Inappropriate,
                _ => Enum.TryParse<EthicsRating>(rating, true, out var parsed) ? parsed : EthicsRating.Appropriate
            };
        }
    }
}
