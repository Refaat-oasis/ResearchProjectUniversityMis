using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using researchProject.Models.Constants;
using researchProject.Models.DTOs;
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
        private readonly IHangfireEmailDispatcher _hangfireEmailDispatcher;
        private readonly IReportExportService _reportExportService;

        public SuperAdminController(
            ISuperAdminRepository superAdminRepo,
            IEmailSender emailSender,
            IHangfireEmailDispatcher hangfireEmailDispatcher,
            IReportExportService? reportExportService = null)
        {
            _superAdminRepo = superAdminRepo;
            _emailSender = emailSender;
            _hangfireEmailDispatcher = hangfireEmailDispatcher;
            _reportExportService = reportExportService ?? new ReportExportService();
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
            ViewBag.SystemUsers = await _superAdminRepo.GetAssignableUsersForInvitationAsync();
            var coAdmins = await _superAdminRepo.GetAllCoAdminsAsync();
            return View(coAdmins);
        }

        [HttpGet]
        public async Task<IActionResult> Researchers()
        {
            var researchers = await _superAdminRepo.GetAllResearchersAsync();
            return View(researchers);
        }

        [HttpGet]
        public async Task<IActionResult> ManageResearchersPartial()
        {
            var researchers = await _superAdminRepo.GetAllResearchersAsync();
            return PartialView("_ManageResearchersPartial", researchers);
        }

        [HttpGet]
        public async Task<IActionResult> Judges()
        {
            ViewBag.Invitations = await _superAdminRepo.GetAllJudgeInvitationsAsync();
            ViewBag.SystemUsers = await _superAdminRepo.GetAssignableUsersForInvitationAsync();
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
        public async Task<IActionResult> CreateCollege(CollegeCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "يرجى استيفاء جميع بيانات الكلية المطلوبة." });
            }

            var college = new College
            {
                Name = dto.Name.Trim(),
                Code = dto.Code.Trim().ToUpperInvariant()
            };

            var (succeeded, errorMessage) = await _superAdminRepo.CreateCollegeAsync(college);
            return Json(new { success = succeeded, message = errorMessage ?? "تمت إضافة الكلية بنجاح." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCollege(CollegeUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "بيانات الكلية غير صالحة." });
            }

            var college = new College
            {
                Id = dto.Id,
                Name = dto.Name.Trim(),
                Code = dto.Code.Trim().ToUpperInvariant()
            };

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
        public async Task<IActionResult> SaveCategory(CategorySaveDto dto)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.Name) || dto.CollegeId <= 0)
            {
                return Json(new { success = false, message = "اسم التصنيف والكلية مطلوبان بصيغة صحيحة." });
            }

            var category = new ResearchCategory
            {
                Id = dto.Id,
                CollegeId = dto.CollegeId,
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                FieldsDefinitionJson = dto.FieldsDefinitionJson
            };

            var (succeeded, errorMessage) = await _superAdminRepo.SaveCategoryAsync(category);
            return Json(new { success = succeeded, message = errorMessage ?? "تم حفظ تصنيف البحث وتعريف الحقول بنجاح." });
        }

        [HttpGet]
        public async Task<IActionResult> ManageCoAdminsPartial()
        {
            ViewBag.Colleges = await _superAdminRepo.GetAllCollegesAsync();
            ViewBag.SystemUsers = await _superAdminRepo.GetAssignableUsersForInvitationAsync();
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

            var (succeeded, errorMessage, tempPassword, newUserId) = await _superAdminRepo.CreateCoAdminNewUserAsync(email, fullName, collegeId);
            if (!succeeded)
            {
                return Json(new { success = false, message = errorMessage ?? "فشل إنشاء الحساب." });
            }

            var college = await _superAdminRepo.GetCollegeByIdAsync(collegeId);
            var collegeName = college?.Name ?? "الكلية";
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            if (!string.IsNullOrEmpty(newUserId) && !string.IsNullOrEmpty(tempPassword))
            {
                _hangfireEmailDispatcher.EnqueueCoAdminCredentialsEmail(newUserId, email, fullName, tempPassword, collegeName, currentUserId);
            }

            return Json(new 
            { 
                success = true, 
                message = $"تم إنشاء حساب مدير الكلية بنجاح وجدولة إرسال كلمة المرور المؤقتة ({tempPassword}) إلى بريده عبر Hangfire." 
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
            ViewBag.SystemUsers = await _superAdminRepo.GetAssignableUsersForInvitationAsync();
            var judges = await _superAdminRepo.GetAllJudgesAsync();
            return PartialView("_ManageJudgesPartial", judges);
        }

        [HttpGet]
        public async Task<IActionResult> GetDepartments(int collegeId)
        {
            var departments = await _superAdminRepo.GetDepartmentsByCollegeAsync(collegeId);
            return Json(departments.Select(d => new { id = d.Id, name = d.Name, collegeId = d.CollegeId }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDepartment(int? id, string name, int collegeId)
        {
            var (succeeded, errorMessage) = await _superAdminRepo.SaveDepartmentAsync(id, name, collegeId);
            if (!succeeded)
            {
                return Json(new { success = false, message = errorMessage ?? "فشل حفظ القسم الأكاديمي." });
            }
            return Json(new { success = true, message = "تم حفظ القسم الأكاديمي بنجاح." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var (succeeded, errorMessage) = await _superAdminRepo.DeleteDepartmentAsync(id);
            if (!succeeded)
            {
                return Json(new { success = false, message = errorMessage ?? "فشل حذف القسم الأكاديمي." });
            }
            return Json(new { success = true, message = "تم حذف القسم الأكاديمي بنجاح." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InviteJudge([FromForm] string? email, [FromForm] string? specialty, [FromForm] int? collegeId = null)
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
                return Json(new { success = false, message = "البريد الإلكتروني إلزامي لإرسال الدعوة." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var (succeeded, errorMessage, rawToken, invitationId) = await _superAdminRepo.InviteJudgeAsync(cleanEmail, specialty, userId, collegeId);

            if (!succeeded)
            {
                return Json(new { success = false, message = errorMessage ?? "فشل إرسال الدعوة." });
            }

            var scheme = Request?.Scheme ?? "https";
            var invitationLink = Url.Action("AcceptJudgeInvitation", "Account", new { token = rawToken }, scheme);
            if (!string.IsNullOrEmpty(invitationLink) && invitationId.HasValue)
            {
                _hangfireEmailDispatcher.EnqueueJudgeInvitationEmail(invitationId.Value, cleanEmail, cleanEmail, invitationLink, userId);
            }

            return Json(new { success = true, message = "تم إنشاء وجدولة إرسال دعوة المحكم بنجاح عبر البريد الإلكتروني." });
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
        public async Task<IActionResult> ToggleResearcherStatus(int researcherId, bool isActive)
        {
            var (succeeded, errorMessage) = await _superAdminRepo.SetResearcherActiveStatusAsync(researcherId, isActive);
            return Json(new { success = succeeded, message = errorMessage ?? (isActive ? "تم تفعيل حساب الباحث بنجاح." : "تم تعطيل حساب الباحث وإلغاء جلساته النشطة.") });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCoAdminStatus(int coAdminId, bool isActive)
        {
            var (succeeded, errorMessage) = await _superAdminRepo.SetCoAdminActiveStatusAsync(coAdminId, isActive);
            return Json(new { success = succeeded, message = errorMessage ?? (isActive ? "تم تفعيل حساب مدير الكلية بنجاح." : "تم تعطيل حساب مدير الكلية وإلغاء جلساته النشطة.") });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReassignCoAdminCollege(int coAdminId, int newCollegeId)
        {
            var (succeeded, errorMessage) = await _superAdminRepo.ReassignCoAdminCollegeAsync(coAdminId, newCollegeId);
            return Json(new { success = succeeded, message = errorMessage ?? "تمت إعادة تعيين ونقل مدير الكلية بنجاح." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCategoryStatus(int id, bool isActive)
        {
            var (succeeded, errorMessage) = await _superAdminRepo.ToggleCategoryStatusAsync(id, isActive);
            return Json(new { success = succeeded, message = errorMessage ?? (isActive ? "تم تفعيل التصنيف." : "تم تعطيل التصنيف وإخفاؤه من استمارة تقديم الباحثين.") });
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
            var bytes = _reportExportService.ExportStatisticsToCsv(stats, "تقرير إحصائيات منصة الأخلاقيات بالجامعة");
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
