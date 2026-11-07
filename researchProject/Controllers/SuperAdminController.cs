using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using researchProject.Models.Constants;
using researchProject.Models.Entities;
using researchProject.Repositories.Interfaces;
using researchProject.Services;

namespace researchProject.Controllers
{
    [Authorize(Policy = "SuperAdminOnly")]
    public class SuperAdminController : Controller
    {
        private readonly ISuperAdminRepository _superAdminRepo;
        private readonly IEmailSender _emailSender;

        public SuperAdminController(ISuperAdminRepository superAdminRepo, IEmailSender emailSender)
        {
            _superAdminRepo = superAdminRepo;
            _emailSender = emailSender;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var stats = await _superAdminRepo.GetSystemStatisticsAsync();
            return View(stats);
        }

        [HttpGet]
        public async Task<IActionResult> Colleges()
        {
            var colleges = await _superAdminRepo.GetAllCollegesAsync();
            return View(colleges);
        }

        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            ViewBag.Colleges = await _superAdminRepo.GetAllCollegesAsync();
            var categories = await _superAdminRepo.GetAllCategoriesAsync();
            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> CoAdmins()
        {
            ViewBag.Colleges = await _superAdminRepo.GetAllCollegesAsync();
            var coAdmins = await _superAdminRepo.GetAllCoAdminsAsync();
            return View(coAdmins);
        }

        [HttpGet]
        public async Task<IActionResult> Judges()
        {
            ViewBag.Invitations = await _superAdminRepo.GetAllJudgeInvitationsAsync();
            var judges = await _superAdminRepo.GetAllJudgesAsync();
            return View(judges);
        }

        [HttpGet]
        public async Task<IActionResult> Researches()
        {
            var researches = await _superAdminRepo.GetAllResearchesAsync();
            return View(researches);
        }

        [HttpGet]
        public async Task<IActionResult> ResearchDetails(int id)
        {
            var research = await _superAdminRepo.GetResearchDetailsAsync(id);
            if (research == null) return NotFound("المقترح البحثي غير موجود.");
            return View(research);
        }

        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            var stats = await _superAdminRepo.GetSystemStatisticsAsync();
            return View(stats);
        }

        [HttpGet]
        public async Task<IActionResult> Certificates(string? serialNumber, string? researcherName, int? collegeId, DateTime? fromDate, DateTime? toDate)
        {
            ViewBag.Colleges = await _superAdminRepo.GetAllCollegesAsync();
            var archives = await _superAdminRepo.SearchCertificatesArchiveAsync(serialNumber, researcherName, collegeId, fromDate, toDate);
            return View(archives);
        }

        [HttpGet]
        public async Task<IActionResult> OverviewDashboardPartial()
        {
            var stats = await _superAdminRepo.GetSystemStatisticsAsync();
            return PartialView("_OverviewDashboardPartial", stats);
        }

        [HttpGet]
        public async Task<IActionResult> ManageCollegesPartial()
        {
            var colleges = await _superAdminRepo.GetAllCollegesAsync();
            return PartialView("_ManageCollegesPartial", colleges);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCollege(College college)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "يرجى استيفاء جميع بيانات الكلية المطلوبة." });
            }

            var (succeeded, errorMessage) = await _superAdminRepo.CreateCollegeAsync(college);
            return Json(new { success = succeeded, message = errorMessage ?? "تمت إضافة الكلية بنجاح." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCollege(College college)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "بيانات غير صالحة." });
            }

            var (succeeded, errorMessage) = await _superAdminRepo.UpdateCollegeAsync(college);
            return Json(new { success = succeeded, message = errorMessage ?? "تم تحديث بيانات الكلية بنجاح." });
        }

        [HttpGet]
        public async Task<IActionResult> ManageCategoriesPartial()
        {
            ViewBag.Colleges = await _superAdminRepo.GetAllCollegesAsync();
            var categories = await _superAdminRepo.GetAllCategoriesAsync();
            return PartialView("_ManageCategoriesPartial", categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCategory(ResearchCategory category)
        {
            if (string.IsNullOrWhiteSpace(category.Name) || category.CollegeId <= 0)
            {
                return Json(new { success = false, message = "اسم التصنيف والكلية مطلوبان." });
            }

            var (succeeded, errorMessage) = await _superAdminRepo.SaveCategoryAsync(category);
            return Json(new { success = succeeded, message = errorMessage ?? "تم حفظ تصنيف البحث وتعريف الحقول بنجاح." });
        }

        [HttpGet]
        public async Task<IActionResult> ManageCoAdminsPartial()
        {
            ViewBag.Colleges = await _superAdminRepo.GetAllCollegesAsync();
            var coAdmins = await _superAdminRepo.GetAllCoAdminsAsync();
            return PartialView("_ManageCoAdminsPartial", coAdmins);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCoAdminNewUser(string email, string fullName, int collegeId)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(fullName) || collegeId <= 0)
            {
                return Json(new { success = false, message = "جميع حقول البريد والاسم والكلية مطلوبة." });
            }

            var (succeeded, errorMessage, tempPassword) = await _superAdminRepo.CreateCoAdminNewUserAsync(email, fullName, collegeId);
            return Json(new 
            { 
                success = succeeded, 
                message = errorMessage ?? $"تم إنشاء حساب مدير الكلية بنجاح وإرسال كلمة المرور المؤقتة ({tempPassword}) إلى بريده." 
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCoAdminExistingUser(string userId, int collegeId)
        {
            if (string.IsNullOrWhiteSpace(userId) || collegeId <= 0)
            {
                return Json(new { success = false, message = "معرف المستخدم والكلية مطلوبان." });
            }

            var (succeeded, errorMessage) = await _superAdminRepo.AddCoAdminRoleToExistingUserAsync(userId, collegeId);
            return Json(new { success = succeeded, message = errorMessage ?? "تمت إضافة صلاحية مدير لجنة الكلية للمستخدم بنجاح." });
        }

        [HttpGet]
        public async Task<IActionResult> ManageJudgesPartial()
        {
            ViewBag.Invitations = await _superAdminRepo.GetAllJudgeInvitationsAsync();
            var judges = await _superAdminRepo.GetAllJudgesAsync();
            return PartialView("_ManageJudgesPartial", judges);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InviteJudge(string email, string? specialty)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(new { success = false, message = "البريد الإلكتروني إلزامي لإرسال الدعوة." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var (succeeded, errorMessage, rawToken) = await _superAdminRepo.InviteJudgeAsync(email, specialty, userId);

            if (!succeeded)
            {
                return Json(new { success = false, message = errorMessage ?? "فشل إرسال الدعوة." });
            }

            var invitationLink = Url.Action("AcceptJudgeInvitation", "Account", new { token = rawToken }, Request.Scheme);
            if (!string.IsNullOrEmpty(invitationLink))
            {
                await _emailSender.SendJudgeInvitationAsync(email, invitationLink, specialty ?? "عام");
            }

            return Json(new { success = true, message = "تم إنشاء وإرسال دعوة المحكم بنجاح عبر البريد الإلكتروني." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleJudgeStatus(int judgeId, bool isActive)
        {
            var (succeeded, errorMessage) = await _superAdminRepo.SetJudgeActiveStatusAsync(judgeId, isActive);
            return Json(new { success = succeeded, message = errorMessage ?? (isActive ? "تم تفعيل المحكم." : "تم تعطيل المحكم وإخفاؤه من قوائم الإسناد.") });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeInvitation(int invitationId)
        {
            var (succeeded, errorMessage) = await _superAdminRepo.RevokeInvitationAsync(invitationId);
            return Json(new { success = succeeded, message = errorMessage ?? "تم إلغاء الدعوة بنجاح." });
        }

        [HttpGet]
        public async Task<IActionResult> AllUniversityResearchesPartial()
        {
            var researches = await _superAdminRepo.GetAllResearchesAsync();
            return PartialView("_AllUniversityResearchesPartial", researches);
        }

        [HttpGet]
        public async Task<IActionResult> ResearchDetailsPartial(int id)
        {
            var research = await _superAdminRepo.GetResearchDetailsAsync(id);
            if (research == null) return NotFound("المقترح البحثي غير موجود.");
            return PartialView("_ResearchDetailsPartial", research);
        }

        [HttpGet]
        public async Task<IActionResult> ReportsPartial()
        {
            var stats = await _superAdminRepo.GetSystemStatisticsAsync();
            return PartialView("_ReportsPartial", stats);
        }

        [HttpGet]
        public async Task<IActionResult> ExportReportsToCsv()
        {
            var stats = await _superAdminRepo.GetSystemStatisticsAsync();

            var sb = new StringBuilder();
            sb.AppendLine("المؤشر,القيمة");
            sb.AppendLine($"إجمالي المقترحات بالجامعة,{stats.TotalResearches}");
            sb.AppendLine($"قيد الفحص الشكلي,{stats.UnderScreeningCount}");
            sb.AppendLine($"مسند للمحكمين,{stats.AssignedToJudgesCount}");
            sb.AppendLine($"تعديلات مطلوبة,{stats.ModificationsRequiredCount}");
            sb.AppendLine($"معتمد,{stats.ApprovedCount}");
            sb.AppendLine($"صادر الشهادة,{stats.CertificatesIssuedCount}");
            sb.AppendLine($"عدد الكليات,{stats.TotalColleges}");
            sb.AppendLine($"عدد المحكمين النشطين,{stats.TotalJudges}");
            sb.AppendLine($"إجمالي الباحثين المسجلين,{stats.TotalResearchers}");
            sb.AppendLine($"متوسط ساعات الفحص الشكلي,{stats.AverageScreeningHours}");
            sb.AppendLine($"متوسط أيام التحكيم الأخلاقي,{stats.AverageJudgingDays}");
            sb.AppendLine($"نسبة الالتزام بالمهلة النظامية,{stats.SlaCompliancePercentage}%");

            if (stats.ResearchesByCollege != null && stats.ResearchesByCollege.Any())
            {
                sb.AppendLine("\nالكلية,عدد المقترحات");
                foreach (var c in stats.ResearchesByCollege)
                {
                    sb.AppendLine($"\"{c.Key}\",{c.Value}");
                }
            }

            if (stats.ResearchesByCategory != null && stats.ResearchesByCategory.Any())
            {
                sb.AppendLine("\nتصنيف البحث,عدد المقترحات");
                foreach (var cat in stats.ResearchesByCategory)
                {
                    sb.AppendLine($"\"{cat.Key}\",{cat.Value}");
                }
            }

            var preamble = Encoding.UTF8.GetPreamble();
            var bytes = preamble.Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv; charset=utf-8", $"University_Ethics_Report_{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> CertificatesArchivePartial(string? serialNumber, string? researcherName, int? collegeId, DateTime? fromDate, DateTime? toDate)
        {
            ViewBag.Colleges = await _superAdminRepo.GetAllCollegesAsync();
            var archives = await _superAdminRepo.SearchCertificatesArchiveAsync(serialNumber, researcherName, collegeId, fromDate, toDate);
            return PartialView("_CertificatesArchivePartial", archives);
        }
    }
}
