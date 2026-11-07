using System.Collections.Generic;

namespace researchProject.Models.ViewModels
{
    public class SystemStatisticsViewModel
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
