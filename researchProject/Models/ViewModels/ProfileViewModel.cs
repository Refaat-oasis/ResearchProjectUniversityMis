using System.Collections.Generic;

namespace researchProject.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
        public string? CollegeName { get; set; }
        public string? Department { get; set; }
        public string? AcademicDegree { get; set; }
        public string? ResearcherType { get; set; }
        public string? NationalId { get; set; }
        public string? Specialty { get; set; }
        public bool HasSignature { get; set; }
    }
}

