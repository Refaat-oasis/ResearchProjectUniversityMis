using researchProject.Models.Enums;

namespace researchProject.Models.Constants
{
    public static class RoleConstants
    {
        public const string SuperAdmin = nameof(UserRole.SuperAdmin);
        public const string CollegeCoAdmin = nameof(UserRole.CollegeCoAdmin);
        public const string Judge = nameof(UserRole.Judge);
        public const string Researcher = nameof(UserRole.Researcher);
        public const string Developer = nameof(UserRole.Developer);
    }
}

