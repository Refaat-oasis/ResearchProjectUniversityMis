using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using researchProject.Models.ViewModels;
using researchProject.Repositories.Interfaces;

namespace researchProject.Controllers
{
    [AllowAnonymous]
    public class PublicController : Controller
    {
        private readonly IPublicRepository _publicRepo;

        public PublicController(IPublicRepository publicRepo)
        {
            _publicRepo = publicRepo;
        }

        [HttpGet]
        public async Task<IActionResult> VerifyCertificate(string? serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                return View(new CertificateVerificationResultViewModel
                {
                    IsValid = false,
                    Message = "يرجى إدخال الرقم المسلسل للشهادة للتحقق من صحتها."
                });
            }

            var result = await _publicRepo.VerifyCertificateAsync(serialNumber);
            return View(result);
        }
    }
}
