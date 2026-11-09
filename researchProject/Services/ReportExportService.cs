using System;
using System.Linq;
using System.Text;
using researchProject.Models.ViewModels;

namespace researchProject.Services
{
    public class ReportExportService : IReportExportService
    {
        public byte[] ExportStatisticsToCsv(SystemStatisticsViewModel stats, string reportTitle)
        {
            var sb = new StringBuilder();
            sb.AppendLine("المؤشر,القيمة");
            sb.AppendLine($"تقرير,{reportTitle}");
            sb.AppendLine($"تاريخ التقرير,{DateTime.UtcNow:yyyy/MM/dd HH:mm} UTC");
            sb.AppendLine($"إجمالي المقترحات بالجامعة,{stats.TotalResearches}");
            sb.AppendLine($"قيد الفحص الشكلي,{stats.UnderScreeningCount}");
            sb.AppendLine($"مسند للمحكمين,{stats.AssignedToJudgesCount}");
            sb.AppendLine($"تعديلات مطلوبة,{stats.ModificationsRequiredCount}");
            sb.AppendLine($"معتمد,{stats.ApprovedCount}");
            sb.AppendLine($"صادر الشهادة,{stats.CertificatesIssuedCount}");

            if (stats.TotalColleges > 0)
            {
                sb.AppendLine($"عدد الكليات,{stats.TotalColleges}");
            }
            if (stats.TotalJudges > 0)
            {
                sb.AppendLine($"عدد المحكمين النشطين,{stats.TotalJudges}");
            }

            sb.AppendLine($"إجمالي الباحثين,{stats.TotalResearchers}");
            sb.AppendLine($"متوسط ساعات الفحص,{stats.AverageScreeningHours}");
            sb.AppendLine($"متوسط أيام التحكيم,{stats.AverageJudgingDays}");
            sb.AppendLine($"نسبة الالتزام بالمهلة النظامية,{stats.SlaCompliancePercentage}%");

            if (stats.ResearchesByCollege != null && stats.ResearchesByCollege.Any())
            {
                sb.AppendLine();
                sb.AppendLine("الكلية,عدد المقترحات");
                foreach (var c in stats.ResearchesByCollege)
                {
                    sb.AppendLine($"\"{c.Key}\",{c.Value}");
                }
            }

            if (stats.ResearchesByCategory != null && stats.ResearchesByCategory.Any())
            {
                sb.AppendLine();
                sb.AppendLine("تصنيف البحث,عدد المقترحات");
                foreach (var cat in stats.ResearchesByCategory)
                {
                    sb.AppendLine($"\"{cat.Key}\",{cat.Value}");
                }
            }

            var preamble = Encoding.UTF8.GetPreamble();
            return preamble.Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        }
    }
}
