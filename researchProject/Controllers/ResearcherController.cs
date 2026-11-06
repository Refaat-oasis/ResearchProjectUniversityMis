using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using researchProject.Models.Constants;
using researchProject.Models.DTOs;
using researchProject.Repositories.Interfaces;
using researchProject.Services;

namespace researchProject.Controllers
{
    [Authorize(Roles = $"{RoleConstants.Researcher},{RoleConstants.SuperAdmin}")]
    public class ResearcherController : Controller
    {
        private readonly IResearcherRepository _researcherRepo;
        private readonly IFileStorageService _fileStorage;
        private readonly IWebHostEnvironment _env;

        public ResearcherController(
            IResearcherRepository researcherRepo,
            IFileStorageService fileStorage,
            IWebHostEnvironment env)
        {
            _researcherRepo = researcherRepo;
            _fileStorage = fileStorage;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var profile = await _researcherRepo.GetProfileByUserIdAsync(userId);
            return View(profile);
        }

        [HttpGet]
        public async Task<IActionResult> MyResearchesPartial()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var researches = await _researcherRepo.GetMyResearchesAsync(userId);
            return PartialView("_MyResearchesPartial", researches);
        }

        [HttpGet]
        public async Task<IActionResult> CreateResearchPartial()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var profile = await _researcherRepo.GetProfileByUserIdAsync(userId);

            if (profile != null)
            {
                ViewBag.Categories = await _researcherRepo.GetCategoriesForCollegeAsync(profile.CollegeId);
                ViewBag.ResearcherCollege = profile.College;
            }

            return PartialView("_CreateResearchPartial", new ResearchCreateDto());
        }

        [HttpGet]
        public async Task<IActionResult> DynamicCategoryFieldsPartial(int categoryId)
        {
            var category = await _researcherRepo.GetCategoryWithFieldsAsync(categoryId);
            return PartialView("_DynamicCategoryFieldsPartial", category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateResearch(ResearchCreateDto dto)
        {
            // Parse dynamic field answers from form data
            foreach (var key in Request.Form.Keys)
            {
                if (key.StartsWith("DynamicFields["))
                {
                    var fieldName = key.Replace("DynamicFields[", "").Replace("]", "");
                    dto.DynamicAnswers[fieldName] = Request.Form[key].ToString();
                }
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "يرجى تعبئة جميع الحقول ورفع المستندات الإلزامية المطلوبة." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var (succeeded, errorMessage, researchId) = await _researcherRepo.CreateResearchWithAttachmentsAsync(dto, userId);

            return Json(new 
            { 
                success = succeeded, 
                message = errorMessage ?? "تم تقديم المقترح البحثي بنجاح، وهو الآن قيد المراجعة الشكلية.", 
                researchId 
            });
        }

        [HttpGet]
        public async Task<IActionResult> ResearchStatusTrackerPartial(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var research = await _researcherRepo.GetResearchDetailsAsync(id, userId);
            if (research == null) return NotFound("المقترح البحثي غير موجود أو غير مصرح لك بمشاهدته.");

            return PartialView("_ResearchStatusTrackerPartial", research);
        }

        [HttpGet]
        public async Task<IActionResult> UploadModificationsPartial(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var research = await _researcherRepo.GetResearchDetailsAsync(id, userId);
            if (research == null) return NotFound("المقترح البحثي غير موجود.");

            if (research.WorkflowStatus != ResearchStatusConstants.ModificationsRequired)
            {
                return BadRequest("هذا البحث ليس بحاجة لتعديلات حالياً.");
            }

            var dto = new ResearchModificationDto { ResearchId = id };
            ViewBag.Research = research;
            return PartialView("_UploadModificationsPartial", dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitModifications(ResearchModificationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "يرجى إرفاق ملف المقترح المعدل." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var (succeeded, errorMessage) = await _researcherRepo.SubmitModifiedAttachmentsAsync(dto, userId);

            return Json(new 
            { 
                success = succeeded, 
                message = errorMessage ?? "تم تسليم الملفات المعدلة بنجاح وإعادة المقترح للمحكمين لبدء جولة تحكيمية جديدة." 
            });
        }

        [HttpGet]
        public async Task<IActionResult> MyCertificatesPartial()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var certs = await _researcherRepo.GetMyCertificatesAsync(userId);
            return PartialView("_MyCertificatesPartial", certs);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadCertificate(int researchId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var research = await _researcherRepo.GetResearchDetailsAsync(researchId, userId);
            if (research == null || string.IsNullOrEmpty(research.CertificateFilePath))
            {
                return NotFound("ملف الشهادة غير موجود أو غير مصرح بتحميله.");
            }

            var certDir = Path.Combine(_env.ContentRootPath, "App_Data", "Certificates");
            var fullPath = Path.Combine(certDir, research.CertificateFilePath);

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound("الملف غير متوفر في الخادم.");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(fileBytes, "text/html", $"Certificate_{research.CertificateSerialNumber}.html");
        }
    }
}
