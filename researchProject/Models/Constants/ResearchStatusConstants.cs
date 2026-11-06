namespace researchProject.Models.Constants
{
    public static class ResearchStatusConstants
    {
        public const string UnderFormalScreening = "قيد المراجعة الشكلية";
        public const string AssignedToJudges = "مسند للمحكمين";
        public const string ModificationsRequired = "تعديلات مطلوبة";
        public const string Approved = "معتمد";
        public const string CertificateIssued = "صادر الشهادة";
    }

    public static class RoleConstants
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string CollegeCoAdmin = "CollegeCoAdmin";
        public const string Judge = "Judge";
        public const string Researcher = "Researcher";
    }

    public static class InvitationStatusConstants
    {
        public const string Pending = "معلق";
        public const string Accepted = "مقبول";
        public const string Expired = "منتهي";
        public const string Revoked = "ملغى";
    }

    public static class ResearcherTypeConstants
    {
        public const string FacultyMember = "عضو هيئة تدريس";
        public const string Researcher = "باحث";
        public const string PostgraduateStudent = "طالب دراسات عليا";
    }

    public static class AttachmentTypeConstants
    {
        public const string InformedConsent = "الموافقة المستنيرة";
        public const string ResearchTools = "أدوات البحث";
        public const string ResearchProposal = "خطة البحث";
        public const string AdditionalDocument = "مستند إضافي";
        public const string ModifiedFile = "ملف معدل";
    }

    public static class EvaluationDecisionConstants
    {
        public const string UnderEvaluation = "قيد التقييم";
        public const string Approved = "موافق";
        public const string ModificationsRequired = "طلب تعديلات";
    }

    public static class EthicsRatingConstants
    {
        public const string Appropriate = "مناسب";
        public const string NeedsModification = "يحتاج تعديل";
        public const string Inappropriate = "غير مناسب";
    }
}
