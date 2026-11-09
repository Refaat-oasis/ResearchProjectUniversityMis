using researchProject.Models.ViewModels;

namespace researchProject.Services
{
    public interface IReportExportService
    {
        byte[] ExportStatisticsToCsv(SystemStatisticsViewModel stats, string reportTitle);
    }
}
