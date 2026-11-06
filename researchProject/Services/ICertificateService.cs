using researchProject.Models.Entities;

namespace researchProject.Services
{
    public interface ICertificateService
    {
        string GenerateSerialNumber(string collegeCode);
        string GenerateQrCodeBase64(string text);
        Task<string> GenerateCertificateFileAsync(Research research, CollegeCoAdmin issuingCoAdmin, string baseUrl);
    }
}
