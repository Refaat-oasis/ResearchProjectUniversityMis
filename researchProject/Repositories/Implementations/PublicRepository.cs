using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using researchProject.Data;
using researchProject.Models.Constants;
using researchProject.Models.DTOs;
using researchProject.Repositories.Interfaces;

namespace researchProject.Repositories.Implementations
{
    public class PublicRepository : IPublicRepository
    {
        private readonly ApplicationDbContext _db;

        public PublicRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<CertificateVerificationResultDto> VerifyCertificateAsync(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                return new CertificateVerificationResultDto
                {
                    IsValid = false,
                    Message = "رقم الشهادة غير محدد أو غير صالح."
                };
            }

            var research = await _db.Researches
                .Include(r => r.College)
                .FirstOrDefaultAsync(r => r.CertificateSerialNumber == serialNumber.Trim() &&
                                         r.WorkflowStatus == ResearchStatusConstants.CertificateIssued);

            if (research == null)
            {
                return new CertificateVerificationResultDto
                {
                    IsValid = false,
                    SerialNumber = serialNumber,
                    Message = "هذا الرقم المسلسل غير موجود في سجلات الشهادات المعتمدة، أو لم يتم إصدار شهادة له."
                };
            }

            return new CertificateVerificationResultDto
            {
                IsValid = true,
                SerialNumber = research.CertificateSerialNumber,
                ResearchTitle = research.Title,
                CollegeName = research.College?.Name ?? "غير محدد",
                IssueDate = research.CertificateIssuedAt,
                Message = "تم التحقق بنجاح: هذه الشهادة رسمية ومعتمدة من اللجنة الفرعية لأخلاقيات البحث العلمي والنزاهة الأكاديمية."
            };
        }
    }
}
