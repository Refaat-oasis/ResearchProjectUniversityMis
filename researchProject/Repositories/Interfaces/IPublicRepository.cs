using System.Threading.Tasks;
using researchProject.Models.ViewModels;

namespace researchProject.Repositories.Interfaces
{
    public interface IPublicRepository
    {
        Task<CertificateVerificationResultViewModel> VerifyCertificateAsync(string serialNumber);
    }
}
