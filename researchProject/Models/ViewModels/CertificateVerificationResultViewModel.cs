using System;

namespace researchProject.Models.ViewModels
{
    public class CertificateVerificationResultViewModel
    {
        public bool IsValid { get; set; }
        public string? SerialNumber { get; set; }
        public string? ResearchTitle { get; set; }
        public string? CollegeName { get; set; }
        public DateTime? IssueDate { get; set; }
        public string? Message { get; set; }
    }
}

