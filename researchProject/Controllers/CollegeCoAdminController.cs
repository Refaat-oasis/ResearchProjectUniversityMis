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
using researchProject.Repositories.Interfaces;
using researchProject.Services;

namespace researchProject.Controllers
{
    [Authorize(Policy = "CoAdminOrSuperAdmin")]
    public class CollegeCoAdminController : Controller
    {
        private readonly ICollegeCoAdminRepository _coAdminRepo;
        private readonly IEmailSender _emailSender;
        private readonly IReportExportService _reportExportService;

        public CollegeCoAdminController(
            ICollegeCoAdminRepository coAdminRepo,
            IEmailSender emailSender,
            IReportExportService? reportExportService = null)
        {
            _coAdminRepo = coAdminRepo;
            _emailSender = emailSender;
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
        public async Task<IActionResult> InviteJudge(string email, string? specialty)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(new { success = false, message = "البريد الإلكتروني إلزامي." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var (succeeded, errorMessage, rawToken) = await _coAdminRepo.InviteJudgeAsync(email, specialty, userId);

            if (!succeeded)
            {
                return Json(new { success = false, message = errorMessage ?? "فشل إرسال الدعوة." });
            }

            var link = Url.Action("AcceptJudgeInvitation", "Account", new { token = rawToken }, Request.Scheme);
            if (!string.IsNullOrEmpty(link))
            {
                await _emailSender.SendJudgeInvitationAsync(email, link, specialty ?? "عام");
            }

            return Json(new { success = true, message = "تم إرسال دعوة المحكم بنجاح عبر البريد الإلكتروني." });
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
