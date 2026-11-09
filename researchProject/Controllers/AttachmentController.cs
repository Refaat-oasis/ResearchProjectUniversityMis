using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using researchProject.Services.Interfaces;

namespace researchProject.Controllers
{
    [Authorize]
    public class AttachmentController : Controller
    {
        private readonly IAttachmentService _attachmentService;

        public AttachmentController(IAttachmentService attachmentService)
        {
            _attachmentService = attachmentService;
        }

        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            if (id <= 0)
            {
                return BadRequest("معرف المرفق غير صالح.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var (isAuthorized, fileExists, fileStream, contentType, fileName) = await _attachmentService.GetAttachmentDownloadAsync(id, userId);

            if (!isAuthorized)
            {
                return Forbid();
            }

            if (!fileExists || fileStream == null)
            {
                return NotFound("الملف المطلوب غير موجود على الخادم أو تم حذفه.");
            }

            return File(fileStream, contentType, fileName);
        }
    }
}
