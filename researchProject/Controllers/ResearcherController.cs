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
    [Authorize(Policy = "ResearcherOrSuperAdmin")]
    public class ResearcherController : Controller
    {
        private readonly IResearcherRepository _researcherRepo;

        public ResearcherController(IResearcherRepository researcherRepo)
        {
            _researcherRepo = researcherRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return RedirectToAction(nameof(MyResearches));
        }

        [HttpGet]
        public async Task<IActionResult> MyResearches()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var researches = await _researcherRepo.GetMyResearchesAsync(userId);
            return View(researches);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var profile = await _researcherRepo.GetProfileByUserIdAsync(userId);

            if (profile != null)
            {
                ViewBag.Categories = await _researcherRepo.GetCategoriesForCollegeAsync(profile.CollegeId);
                ViewBag.ResearcherCollege = profile.College;
            }

            return View(new ResearchCreateDto());
        }

        [HttpGet]
        public async Task<IActionResult> Tracker(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var research = await _researcherRepo.GetResearchDetailsAsync(id, userId);
            if (research == null) return NotFound("المقترح البحثي غير موجود أو غير مصرح لك بمشاهدته.");

            return View(research);
        }

        [HttpGet]
        public async Task<IActionResult> Modify(int id)
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
            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Certificates()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var certs = await _researcherRepo.GetMyCertificatesAsync(userId);
            return View(certs);
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
            // Parse dynamic field answers from form data if form is available
            if (Request?.HasFormContentType == true && Request.Form != null)
            {
                foreach (var key in Request.Form.Keys)
                {
                    if (key.StartsWith("DynamicFields["))
                    {
                        var fieldName = key.Replace("DynamicFields[", "").Replace("]", "");
                        dto.DynamicAnswers[fieldName] = Request.Form[key].ToString();
                    }
                }
            }

            if (dto.CategoryId <= 0)
            {
                ModelState.AddModelError(nameof(dto.CategoryId), "يرجى اختيار نوع وتصنيف البحث من القائمة المتاحة.");
            }

            if (dto.InformedConsentFile == null || dto.InformedConsentFile.Length == 0)
            {
                ModelState.AddModelError(nameof(dto.InformedConsentFile), "ملف وثيقة الموافقة المستنيرة (Informed Consent) فارغ أو لم يتم إرفاقه.");
            }
            if (dto.ResearchToolsFile == null || dto.ResearchToolsFile.Length == 0)
            {
                ModelState.AddModelError(nameof(dto.ResearchToolsFile), "ملف أدوات وأجهزة البحث (Research Tools) فارغ أو لم يتم إرفاقه.");
            }
            if (dto.ResearchProposalFile == null || dto.ResearchProposalFile.Length == 0)
            {
                ModelState.AddModelError(nameof(dto.ResearchProposalFile), "ملف خطة المقترح البحثي الكاملة (Research Proposal) فارغ أو لم يتم إرفاقه.");
            }
            if (dto.AdditionalDocumentFile != null && dto.AdditionalDocumentFile.Length == 0)
            {
                ModelState.AddModelError(nameof(dto.AdditionalDocumentFile), "الملف الإضافي المرفق فارغ (حجمه 0 بايت).");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(m => !string.IsNullOrWhiteSpace(m))
                    .ToList();
                var errorMsg = errors.Any() 
                    ? string.Join(" | ", errors) 
                    : "يرجى تعبئة جميع الحقول ورفع المستندات الإلزامية المطلوبة.";
                return Json(new { success = false, message = errorMsg });
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
            var (fileBytes, serialNumber) = await _researcherRepo.GetCertificateDownloadAsync(researchId, userId);
            if (fileBytes == null || fileBytes.Length == 0)
            {
                return NotFound("ملف الشهادة غير موجود أو غير مصرح بتحميله.");
            }

            return File(fileBytes, "text/html", $"Certificate_{serialNumber}.html");
        }
    }
}
