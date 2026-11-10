using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using researchProject.Models.Constants;
using researchProject.Models.DTOs;
using researchProject.Repositories.Interfaces;
using researchProject.Services;

namespace researchProject.Controllers
{
    [Authorize(Policy = "CoAdminOrSuperAdmin")]
    public class CollegeCoAdminController : Controller
    {
        private readonly ICollegeCoAdminRepository _coAdminRepo;
        private readonly IEmailSender _emailSender;
        private readonly IHangfireEmailDispatcher _hangfireEmailDispatcher;
        private readonly IReportExportService _reportExportService;

        public CollegeCoAdminController(
            ICollegeCoAdminRepository coAdminRepo,
            IEmailSender emailSender,
            IHangfireEmailDispatcher hangfireEmailDispatcher,
            IReportExportService? reportExportService = null)
        {
            _coAdminRepo = coAdminRepo;
            _emailSender = emailSender;
            _hangfireEmailDispatcher = hangfireEmailDispatcher;
            _reportExportService = reportExportService ?? new ReportExportService();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return RedirectToAction(nameof(ScreeningQueue));
        }

        [HttpGet]
        public async Task<IActionResult> ScreeningQueue()
        {
            var collegeId = await GetCurrentCollegeIdAsync();
            var queue = await _coAdminRepo.GetScreeningQueueAsync(collegeId);
            return View(queue);
        }

        [HttpGet]
        public async Task<IActionResult> Review(int id)
        {
            var collegeId = await GetCurrentCollegeIdAsync();
            var research = await _coAdminRepo.GetResearchDetailsAsync(id, collegeId);
            if (research == null) return NotFound("المقترح البحثي غير موجود أو يتبع كلية أخرى.");

            ViewBag.SuggestedJudges = await _coAdminRepo.SuggestJudgesAsync(id, collegeId);
            ViewBag.AllJudges = await _coAdminRepo.GetAvailableJudgesForCollegeAsync(id);
            return View(research);
        }

        [HttpGet]
        public async Task<IActionResult> FinalApproval()
        {
            var collegeId = await GetCurrentCollegeIdAsync();
            var approvedList = await _coAdminRepo.GetApprovedResearchesForIssuanceAsync(collegeId);
            return View(approvedList);
        }

        [HttpGet]
        public async Task<IActionResult> InviteJudge()
        {
            ViewBag.SystemUsers = await _coAdminRepo.GetAssignableUsersForInvitationAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            var collegeId = await GetCurrentCollegeIdAsync();
            var stats = await _coAdminRepo.GetCollegeStatisticsAsync(collegeId);
            return View(stats);
        }

        [HttpGet]
        public async Task<IActionResult> ScreeningQueuePartial()
        {
            var collegeId = await GetCurrentCollegeIdAsync();
            var queue = await _coAdminRepo.GetScreeningQueueAsync(collegeId);
            return PartialView("_ScreeningQueuePartial", queue);
        }

        [HttpGet]
        public async Task<IActionResult> ResearchReviewPartial(int id)
        {
            var collegeId = await GetCurrentCollegeIdAsync();
            var research = await _coAdminRepo.GetResearchDetailsAsync(id, collegeId);
            if (research == null) return NotFound("المقترح البحثي غير موجود أو يتبع كلية أخرى.");

            ViewBag.SuggestedJudges = await _coAdminRepo.SuggestJudgesAsync(id, collegeId);
            ViewBag.AllJudges = await _coAdminRepo.GetAvailableJudgesForCollegeAsync(id);
            return PartialView("_ResearchReviewPartial", research);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveScreeningNotes(int researchId, string notes)
        {
            var collegeId = await GetCurrentCollegeIdAsync();
            var (succeeded, errorMessage) = await _coAdminRepo.SaveScreeningNotesAsync(researchId, collegeId, notes);
            return Json(new { success = succeeded, message = errorMessage ?? "تم حفظ الملاحظات بنجاح." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignJudges(int researchId, [FromBody] List<int> judgeIds)
        {
            var collegeId = await GetCurrentCollegeIdAsync();
            var actingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            var (succeeded, errorMessage) = await _coAdminRepo.AssignJudgesToResearchAsync(researchId, collegeId, judgeIds, actingUserId);
            return Json(new { success = succeeded, message = errorMessage ?? "تم إسناد المحكمين بنجاح وتحويل حالة البحث إلى مسند للمحكمين." });
        }

        [HttpGet]
        public async Task<IActionResult> InviteJudgePartial()
        {
            ViewBag.SystemUsers = await _coAdminRepo.GetAssignableUsersForInvitationAsync();
            return PartialView("_InviteJudgePartial");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InviteJudge([FromForm] string? email, [FromForm] string? specialty)
        {
            var cleanEmail = (email ?? "").Trim();
            if (string.IsNullOrWhiteSpace(cleanEmail) && Request?.HasFormContentType == true && Request.Form.ContainsKey("email"))
            {
                cleanEmail = (Request.Form["email"].ToString() ?? "").Trim();
            }
            if (string.IsNullOrWhiteSpace(specialty) && Request?.HasFormContentType == true && Request.Form.ContainsKey("specialty"))
            {
                specialty = Request.Form["specialty"].ToString();
            }

            if (string.IsNullOrWhiteSpace(cleanEmail))
            {
                return Json(new { success = false, message = "البريد الإلكتروني إلزامي." });
            }

            var emailValidator = new System.ComponentModel.DataAnnotations.EmailAddressAttribute();
            if (!emailValidator.IsValid(cleanEmail))
            {
                return Json(new { success = false, message = "صيغة البريد الإلكتروني المدخل غير صالحة." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var (succeeded, errorMessage, rawToken, invitationId) = await _coAdminRepo.InviteJudgeAsync(cleanEmail, specialty, userId);

            if (!succeeded)
            {
                return Json(new { success = false, message = errorMessage ?? "فشل إرسال الدعوة." });
            }

            var scheme = Request?.Scheme ?? "https";
            var link = Url.Action("AcceptJudgeInvitation", "Account", new { token = rawToken }, scheme);
            if (!string.IsNullOrEmpty(link) && invitationId.HasValue)
            {
                _hangfireEmailDispatcher.EnqueueJudgeInvitationEmail(invitationId.Value, cleanEmail, cleanEmail, link, userId);
            }

            return Json(new { success = true, message = "تم جدولة إرسال دعوة المحكم بنجاح عبر البريد الإلكتروني." });
        }

        [HttpGet]
        public async Task<IActionResult> FinalApprovalPartial()
        {
            var collegeId = await GetCurrentCollegeIdAsync();
            var approvedList = await _coAdminRepo.GetApprovedResearchesForIssuanceAsync(collegeId);
            return PartialView("_FinalApprovalPartial", approvedList);
        }

        // Exclusive to CollegeCoAdmin ONLY (Blocked for SuperAdmin per Rule Section 4 & 8)
        [HttpPost]
        [Authorize(Policy = "CollegeCoAdminOnly")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IssueCertificate(int researchId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var (succeeded, errorMessage, serialNumber) = await _coAdminRepo.IssueEthicalCertificateAsync(researchId, userId, baseUrl);
            return Json(new 
            { 
                success = succeeded, 
                message = errorMessage ?? $"تم إصدار الشهادة الأخلاقية الرقمية بنجاح برقم مسلسل: {serialNumber}، وإرسالها إلكترونياً للباحثين." 
            });
        }

        [HttpPost]
        [Authorize(Policy = "CollegeCoAdminOnly")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadSignature(IFormFile signatureFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var (succeeded, errorMessage) = await _coAdminRepo.UploadSignatureFileAsync(userId, signatureFile);

            return Json(new { success = succeeded, message = errorMessage ?? "تم رفع وتحديث صورة توقيع مدير اللجنة بنجاح." });
        }

        [HttpGet]
        public async Task<IActionResult> CollegeReportsPartial()
        {
            var collegeId = await GetCurrentCollegeIdAsync();
            var stats = await _coAdminRepo.GetCollegeStatisticsAsync(collegeId);
            return PartialView("_CollegeReportsPartial", stats);
        }

        [HttpGet]
        public async Task<IActionResult> ExportReportsToCsv()
        {
            var collegeId = await GetCurrentCollegeIdAsync();
            var stats = await _coAdminRepo.GetCollegeStatisticsAsync(collegeId);
            var bytes = _reportExportService.ExportStatisticsToCsv(stats, "تقرير إحصائيات كلية");
            return File(bytes, "text/csv; charset=utf-8", $"College_Ethics_Report_{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var collegeId = await GetCurrentCollegeIdAsync();
            var categories = await _coAdminRepo.GetCategoriesForCollegeAsync(collegeId);
            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> ManageCategoriesPartial()
        {
            var collegeId = await GetCurrentCollegeIdAsync();
            var categories = await _coAdminRepo.GetCategoriesForCollegeAsync(collegeId);
            return PartialView("_ManageCategoriesPartial", categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCategory(CategorySaveDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return Json(new { success = false, message = "اسم التصنيف مطلوب ولا يمكن تركه فارغاً." });
            }

            var collegeId = await GetCurrentCollegeIdAsync();
            dto.CollegeId = collegeId; // Lock strictly to the CoAdmin's college
            var (succeeded, errorMessage) = await _coAdminRepo.SaveCategoryAsync(dto, collegeId);
            if (!succeeded)
            {
                return Json(new { success = false, message = errorMessage ?? "فشل حفظ تصنيف البحث." });
            }

            return Json(new { success = true, message = "تم حفظ تصنيف البحث والحقول التفاعلية بنجاح." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCategoryStatus(int id, bool isActive)
        {
            var collegeId = await GetCurrentCollegeIdAsync();
            var (succeeded, errorMessage) = await _coAdminRepo.ToggleCategoryStatusAsync(id, collegeId, isActive);
            if (!succeeded)
            {
                return Json(new { success = false, message = errorMessage ?? "فشل تغيير حالة تفعيل التصنيف." });
            }

            return Json(new { success = true, message = isActive ? "تم تفعيل تصنيف البحث بنجاح وإتاحته للباحثين." : "تم تعطيل تصنيف البحث وإخفاؤه من نموذج التقديم." });
        }

        private async Task<int> GetCurrentCollegeIdAsync()
        {
            if (User.IsInRole(RoleConstants.SuperAdmin))
            {
                return 0; // SuperAdmin sees system-wide
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var profile = await _coAdminRepo.GetProfileByUserIdAsync(userId);
            return profile?.CollegeId ?? -1;
        }
    }
}
