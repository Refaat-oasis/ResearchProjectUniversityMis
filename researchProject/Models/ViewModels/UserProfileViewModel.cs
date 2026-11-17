using System.Collections.Generic;
using researchProject.Models.Entities;

namespace researchProject.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();

        public bool IsSuperAdmin { get; set; }
        public bool IsCollegeCoAdmin { get; set; }
        public bool IsJudge { get; set; }
        public bool IsResearcher { get; set; }

        public bool CanBecomeResearcher => IsJudge && !IsResearcher;

        // Judge Profile details
        public string? JudgeSpecialty { get; set; }
        public string? JudgeAcademicTitle { get; set; }
        public bool JudgeIsActive { get; set; }

        // Researcher Profile details
        public string? ResearcherCollegeName { get; set; }
        public string? ResearcherDepartment { get; set; }
        public string? ResearcherType { get; set; }
        public string? ResearcherDegree { get; set; }
        public string? ResearcherNationalId { get; set; }

        // College list for modal dropdown
        public List<College> AvailableColleges { get; set; } = new List<College>();
    }
}

