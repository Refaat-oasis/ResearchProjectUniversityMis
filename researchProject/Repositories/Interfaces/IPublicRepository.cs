using System.Threading.Tasks;
using researchProject.Models.DTOs;

namespace researchProject.Repositories.Interfaces
{
    public interface IPublicRepository
    {
        Task<CertificateVerificationResultDto> VerifyCertificateAsync(string serialNumber);
    }
}
