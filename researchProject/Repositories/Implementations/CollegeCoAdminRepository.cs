using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using researchProject.Data;
using researchProject.Models.Constants;
using researchProject.Models.DTOs;
using researchProject.Models.Entities;
using researchProject.Models.ViewModels;
using researchProject.Repositories.Interfaces;
using researchProject.Services;
using researchProject.Services.Interfaces;

namespace researchProject.Repositories.Implementations
{
    public class CollegeCoAdminRepository : ICollegeCoAdminRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ICertificateService _certificateService;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CollegeCoAdminRepository> _logger;
        private readonly IWebHostEnvironment? _env;
        private readonly INotificationService? _notificationService;

        public CollegeCoAdminRepository(
            ApplicationDbContext db,
            ICertificateService certificateService,
            IEmailSender emailSender,
            IConfiguration configuration,
            ILogger<CollegeCoAdminRepository> logger,
            IWebHostEnvironment? env = null,
            INotificationService? notificationService = null)
        {
            _db = db;
            _certificateService = certificateService;
            _emailSender = emailSender;
            _configuration = configuration;
            _logger = logger;
            _env = env;
            _notificationService = notificationService;
        }

        public async Task<CollegeCoAdmin?> GetProfileByUserIdAsync(string userId)
        {
            return await _db.CollegeCoAdmins
                .Include(a => a.College)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.UserId == userId);
        }

        public async Task<List<Research>> GetScreeningQueueAsync(int collegeId)
        {
            return await _db.Researches
                .Include(r => r.Category)
                .Include(r => r.ResearcherResearchesBridges).ThenInclude(b => b.Researcher).ThenInclude(res => res.User)
                .Where(r => (collegeId == 0 || r.CollegeId == collegeId) && r.WorkflowStatus == ResearchStatusConstants.UnderFormalScreening)
                .OrderBy(r => r.ScreeningDueAt)
                .ToListAsync();
        }

        public async Task<Research?> GetResearchDetailsAsync(int researchId, int collegeId)
        {
            return await _db.Researches
                .Include(r => r.College)
                .Include(r => r.Category)
                .Include(r => r.ResearcherResearchesBridges).ThenInclude(b => b.Researcher).ThenInclude(res => res.User)
                .Include(r => r.ResearchAttachments)
                .Include(r => r.Evaluations).ThenInclude(e => e.Judge).ThenInclude(j => j.User)
                .Include(r => r.ResearchStatusHistories).ThenInclude(h => h.ChangedByUser)
                .FirstOrDefaultAsync(r => r.Id == researchId && (collegeId == 0 || r.CollegeId == collegeId));
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> SaveScreeningNotesAsync(int researchId, int collegeId, string notes)
        {
            var research = await _db.Researches.FirstOrDefaultAsync(r => r.Id == researchId && (collegeId == 0 || r.CollegeId == collegeId));
            if (research == null) return (false, "المقترح البحثي غير موجود أو لا يتبع هذه الكلية.");

            research.ScreeningNotes = notes;
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<List<Judge>> SuggestJudgesAsync(int researchId, int collegeId)
        {
            var research = await _db.Researches.FirstOrDefaultAsync(r => r.Id == researchId);
            if (research == null) return new List<Judge>();

            // Get user IDs of all authors of this research to enforce conflict of interest
            var authorUserIds = await _db.ResearcherResearchesBridge
                .Where(b => b.ResearchId == researchId)
                .Select(b => b.Researcher.UserId)
                .ToListAsync();

            // Active judges university-wide who are NOT authors of this research
            var activeJudges = await _db.Judges
                .Include(j => j.User)
                .Include(j => j.Evaluations)
                .Where(j => j.IsActive && !authorUserIds.Contains(j.UserId))
                .ToListAsync();

            // Rank by: (1) Specialty matches Research.AcademicSpecialty, (2) Lowest active evaluations count
            return activeJudges
                .OrderByDescending(j => j.Specialty.Trim().Equals(research.AcademicSpecialty.Trim(), StringComparison.OrdinalIgnoreCase))
                .ThenBy(j => j.Evaluations.Count(e => e.Decision == EvaluationDecisionConstants.UnderEvaluation))
                .ToList();
        }

        public async Task<List<Judge>> GetAvailableJudgesForCollegeAsync(int researchId)
        {
            var authorUserIds = await _db.ResearcherResearchesBridge
                .Where(b => b.ResearchId == researchId)
                .Select(b => b.Researcher.UserId)
                .ToListAsync();

            return await _db.Judges
                .Include(j => j.User)
                .Where(j => j.IsActive && !authorUserIds.Contains(j.UserId))
                .OrderBy(j => j.User.FullName)
                .ToListAsync();
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> AssignJudgesToResearchAsync(int researchId, int collegeId, List<int> judgeIds, string actingUserId)
        {
            var minJudges = _configuration.GetValue<int>("Evaluation:MinJudgesPerResearch", 1);
            if (judgeIds == null || judgeIds.Count < minJudges)
            {
                return (false, $"يجب اختيار محكم واحد على الأقل لإسناد البحث (الحد الأدنى: {minJudges}).");
            }

            var research = await _db.Researches
                .Include(r => r.College)
                .FirstOrDefaultAsync(r => r.Id == researchId && (collegeId == 0 || r.CollegeId == collegeId));

            if (research == null) return (false, "المقترح البحثي غير موجود أو يتبع كلية أخرى.");

            if (research.WorkflowStatus != ResearchStatusConstants.UnderFormalScreening &&
                research.WorkflowStatus != ResearchStatusConstants.ModificationsRequired &&
                research.WorkflowStatus != ResearchStatusConstants.AssignedToJudges)
            {
                return (false, "لا يمكن إسناد المحكمين إلا للأبحاث التي هي قيد المراجعة الشكلية، أو تم تسليم تعديلاتها، أو المسندة لإعادة التوزيع.");
            }

            // Conflict of Interest strict check: None of the judges can be an author
            var authorUserIds = await _db.ResearcherResearchesBridge
                .Where(b => b.ResearchId == researchId)
                .Select(b => b.Researcher.UserId)
                .ToListAsync();

            var judges = await _db.Judges
                .Include(j => j.User)
                .Where(j => judgeIds.Contains(j.Id) && j.IsActive)
                .ToListAsync();

            foreach (var judge in judges)
            {
                if (authorUserIds.Contains(judge.UserId))
                {
                    return (false, $"لا يمكن إسناد البحث للمحكم ({judge.User.FullName}) نظراً لوجود تضارب مصالح (أحد الباحثين في هذا المقترح).");
                }
            }

            // Determine next round number
            var currentMaxRound = await _db.Evaluations
                .Where(e => e.ResearchId == researchId)
                .Select(e => (int?)e.RoundNumber)
                .MaxAsync() ?? 0;

            var targetRound = currentMaxRound + 1;

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                foreach (var judge in judges)
                {
                    var evaluation = new Evaluation
                    {
                        ResearchId = researchId,
                        JudgeId = judge.Id,
                        RoundNumber = targetRound,
                        Decision = EvaluationDecisionConstants.UnderEvaluation,
                        IsDigitallySigned = false
                    };
                    await _db.Evaluations.AddAsync(evaluation);
                }

                var oldStatus = research.WorkflowStatus;
                research.WorkflowStatus = ResearchStatusConstants.AssignedToJudges;

                var history = new ResearchStatusHistory
                {
                    ResearchId = researchId,
                    OldStatus = oldStatus,
                    NewStatus = ResearchStatusConstants.AssignedToJudges,
                    ChangedByUserId = actingUserId,
                    ChangedAt = DateTime.UtcNow,
                    Reason = $"إسناد البحث إلى {judges.Count} محكمين للجولة التحكيمية رقم {targetRound}"
                };
                await _db.ResearchStatusHistories.AddAsync(history);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                // Send email notifications to judges
                foreach (var judge in judges)
                {
                    if (!string.IsNullOrEmpty(judge.User.Email))
                    {
                        _ = _emailSender.SendStatusChangedNotificationAsync(
                            judge.User.Email,
                            research.Title,
                            oldStatus,
                            ResearchStatusConstants.AssignedToJudges,
                            $"تم إسناد المقترح البحثي إليكم للتقييم والتحكيم الأخلاقي (الجولة {targetRound}).");
                    }
                }

                // In-app SignalR notifications
                if (_notificationService != null)
                {
                    // 1. Notify each assigned Judge with direct link to evaluation form
                    var newEvaluations = await _db.Evaluations
                        .Where(e => e.ResearchId == researchId && e.RoundNumber == targetRound)
                        .ToListAsync();

                    foreach (var judge in judges)
                    {
                        var eval = newEvaluations.FirstOrDefault(e => e.JudgeId == judge.Id);
                        var evalUrl = eval != null ? $"/Judge/Evaluate/{eval.Id}" : "/Judge/AssignedResearches";

                        await _notificationService.SendNotificationAsync(
                            judge.UserId,
                            "تكليف بتحكيم مقترح بحثي جديد",
                            $"تم تكليفكم رسمياً بتحكيم المقترح البحثي: '{research.Title}' (الجولة التحكيمية {targetRound}).",
                            evalUrl,
                            "JudgeAssignment",
                            "bi-patch-check");
                    }

                    // 2. Notify Researcher that proposal is assigned to judges
                    var authors = await _db.ResearcherResearchesBridge
                        .Where(b => b.ResearchId == researchId)
                        .Include(b => b.Researcher)
                        .ToListAsync();

                    foreach (var author in authors)
                    {
                        if (!string.IsNullOrEmpty(author.Researcher?.UserId))
                        {
                            await _notificationService.SendNotificationAsync(
                                author.Researcher.UserId,
                                "إحالة المقترح للتحكيم الأخلاقي",
                                $"اجتاز مقترحكم '{research.Title}' الفرز الإداري وتمت إحالته لهيئة التحكيم الأكاديمية.",
                                $"/Researcher/Tracker/{researchId}",
                                "StatusChange",
                                "bi-journal-check");
                        }
                    }
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"فشل إسناد المحكمين: {ex.Message}");
            }
        }

        public async Task<List<UserSummaryViewModel>> GetAssignableUsersForInvitationAsync()
        {
            var users = await _db.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var userIds = users.Select(u => u.Id).ToList();
            var userRoles = await _db.UserRoles
                .Where(ur => userIds.Contains(ur.UserId))
                .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, RoleName = r.Name })
                .ToListAsync();

            var roleLookup = userRoles.GroupBy(ur => ur.UserId)
                .ToDictionary(g => g.Key, g => g.First().RoleName);

            var list = new List<UserSummaryViewModel>();
            foreach (var u in users)
            {
                roleLookup.TryGetValue(u.Id, out var roleName);
                list.Add(new UserSummaryViewModel
                {
                    Email = u.Email ?? string.Empty,
                    FullName = string.IsNullOrWhiteSpace(u.FullName) ? (u.UserName ?? u.Email ?? "") : u.FullName,
                    RoleName = roleName ?? "مستخدم"
                });
            }
            return list;
        }

        public async Task<(bool Succeeded, string? ErrorMessage, string? RawToken)> InviteJudgeAsync(string email, string? specialty, string invitedByUserId)
        {
            var rawTokenBytes = new byte[32];
            RandomNumberGenerator.Fill(rawTokenBytes);
            var rawToken = Convert.ToHexString(rawTokenBytes).ToLowerInvariant();

            var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))).ToLowerInvariant();

            var invitation = new JudgeInvitation
            {
                Email = email,
                Specialty = specialty,
                InvitedByUserId = invitedByUserId,
                TokenHash = tokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                Status = InvitationStatusConstants.Pending
            };

            await _db.JudgeInvitations.AddAsync(invitation);
            await _db.SaveChangesAsync();

            return (true, null, rawToken);
        }

        public async Task<List<Research>> GetApprovedResearchesForIssuanceAsync(int collegeId)
        {
            return await _db.Researches
                .Include(r => r.Category)
                .Include(r => r.College)
                .Include(r => r.ResearcherResearchesBridges).ThenInclude(b => b.Researcher).ThenInclude(res => res.User)
                .Include(r => r.Evaluations).ThenInclude(e => e.Judge).ThenInclude(j => j.User)
                .Where(r => (collegeId == 0 || r.CollegeId == collegeId) && r.WorkflowStatus == ResearchStatusConstants.Approved)
                .OrderByDescending(r => r.SubmittedAt)
                .ToListAsync();
        }

        public async Task<(bool Succeeded, string? ErrorMessage, string? SerialNumber)> IssueEthicalCertificateAsync(int researchId, string coAdminUserId, string baseUrl)
        {
            var coAdmin = await _db.CollegeCoAdmins
                .Include(a => a.User)
                .Include(a => a.College)
                .FirstOrDefaultAsync(a => a.UserId == coAdminUserId);

            if (coAdmin == null)
            {
                return (false, "تعذر التحقق من بيانات مدير اللجنة المصدر.", null);
            }

            var research = await _db.Researches
                .Include(r => r.College)
                .Include(r => r.Category)
                .Include(r => r.ResearcherResearchesBridges).ThenInclude(b => b.Researcher).ThenInclude(res => res.User)
                .Include(r => r.Evaluations)
                .FirstOrDefaultAsync(r => r.Id == researchId);

            if (research == null) return (false, "المقترح البحثي غير موجود.", null);

            // College isolation
            if (research.CollegeId != coAdmin.CollegeId)
            {
                return (false, "غير مصرح لك بإصدار شهادة لبحث يتبع كلية أخرى. الإصدار حصري لمدير لجنة كلية البحث.", null);
            }

            // Strict status check: only "Approved" researches can have certificate issued
            if (research.WorkflowStatus != ResearchStatusConstants.Approved)
            {
                return (false, "لا يمكن إصدار الشهادة إلا بعد اعتماد البحث بالكامل من قِبل جميع المحكمين.", null);
            }

            var serialNumber = _certificateService.GenerateSerialNumber(research.College.Code);
            var qrUrl = $"{baseUrl.TrimEnd('/')}/Public/VerifyCertificate?serialNumber={Uri.EscapeDataString(serialNumber)}";

            research.CertificateSerialNumber = serialNumber;
            research.CertificateQrCodeUrl = qrUrl;

            // Generate physical HTML/PDF file
            var certFileName = await _certificateService.GenerateCertificateFileAsync(research, coAdmin, baseUrl);

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var oldStatus = research.WorkflowStatus;
                research.WorkflowStatus = ResearchStatusConstants.CertificateIssued;
                research.CertificateIssuedAt = DateTime.UtcNow;
                research.CertificateFilePath = certFileName;

                var history = new ResearchStatusHistory
                {
                    ResearchId = researchId,
                    OldStatus = oldStatus,
                    NewStatus = ResearchStatusConstants.CertificateIssued,
                    ChangedByUserId = coAdminUserId,
                    ChangedAt = DateTime.UtcNow,
                    Reason = $"إصدار الشهادة الأخلاقية الرسمية برقم مسلسل: {serialNumber} وتوقيع د. {coAdmin.User.FullName}"
                };
                await _db.ResearchStatusHistories.AddAsync(history);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                // Email PDF notification to researchers (failures do NOT roll back)
                var downloadUrl = $"{baseUrl.TrimEnd('/')}/Researcher/DownloadCertificate?researchId={research.Id}";
                foreach (var bridge in research.ResearcherResearchesBridges)
                {
                    var researcherEmail = bridge.Researcher?.User?.Email;
                    if (!string.IsNullOrEmpty(researcherEmail))
                    {
                        try
                        {
                            _ = _emailSender.SendCertificateIssuedNotificationAsync(
                                researcherEmail,
                                research.Title,
                                serialNumber,
                                downloadUrl);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "فشل إرسال إشعار الشهادة للباحث: {Email}", researcherEmail);
                        }
                    }

                    if (_notificationService != null && !string.IsNullOrEmpty(bridge.Researcher?.UserId))
                    {
                        await _notificationService.SendNotificationAsync(
                            bridge.Researcher.UserId,
                            "اعتماد المقترح وإصدار الشهادة الأخلاقية",
                            $"مبارك! تم اعتماد المقترح '{research.Title}' رسمياً وإصدار الشهادة الأخلاقية الرقمية برقم: {serialNumber}",
                            "/Researcher/Certificates",
                            "CertificateIssued",
                            "bi-award");
                    }
                }

                return (true, null, serialNumber);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"فشل إصدار الشهادة: {ex.Message}", null);
            }
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> UploadSignatureImageAsync(string coAdminUserId, string relativePath)
        {
            var coAdmin = await _db.CollegeCoAdmins.FirstOrDefaultAsync(a => a.UserId == coAdminUserId);
            if (coAdmin == null) return (false, "الملف التعريفي غير موجود.");

            coAdmin.SignatureImagePath = relativePath;
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> UploadSignatureFileAsync(string coAdminUserId, IFormFile signatureFile)
        {
            if (signatureFile == null || signatureFile.Length == 0)
            {
                return (false, "يرجى اختيار ملف صورة التوقيع.");
            }

            var ext = Path.GetExtension(signatureFile.FileName).ToLowerInvariant();
            if (ext != ".png" && ext != ".jpg" && ext != ".jpeg")
            {
                return (false, "صيغة التوقيع يجب أن تكون صورة (PNG, JPG).");
            }

            var webRoot = _env?.WebRootPath ?? Path.Combine(_env?.ContentRootPath ?? Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRoot, "uploads", "signatures");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = $"sig_{coAdminUserId}_{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await signatureFile.CopyToAsync(stream);
            }

            var relativePath = $"~/uploads/signatures/{fileName}";
            return await UploadSignatureImageAsync(coAdminUserId, relativePath);
        }

        public async Task<SystemStatisticsViewModel> GetCollegeStatisticsAsync(int collegeId)
        {
            var researches = await _db.Researches
                .Include(r => r.Category)
                .Where(r => collegeId == 0 || r.CollegeId == collegeId)
                .ToListAsync();

            var total = researches.Count;
            var underScreening = researches.Count(r => r.WorkflowStatus == ResearchStatusConstants.UnderFormalScreening);
            var assigned = researches.Count(r => r.WorkflowStatus == ResearchStatusConstants.AssignedToJudges);
            var modifications = researches.Count(r => r.WorkflowStatus == ResearchStatusConstants.ModificationsRequired);
            var approved = researches.Count(r => r.WorkflowStatus == ResearchStatusConstants.Approved);
            var issued = researches.Count(r => r.WorkflowStatus == ResearchStatusConstants.CertificateIssued);

            var totalResearchers = await _db.Researchers.CountAsync(r => collegeId == 0 || r.CollegeId == collegeId);

            var byCategory = researches
                .GroupBy(r => r.Category.Name)
                .ToDictionary(g => g.Key, g => g.Count());

            // Dynamic SLA calculation from ResearchStatusHistories
            var researchIds = researches.Select(r => r.Id).ToList();
            var slaHours = _configuration.GetValue<int>("Screening:SlaHours", 48);

            var histories = await _db.ResearchStatusHistories
                .Where(h => h.OldStatus == ResearchStatusConstants.UnderFormalScreening && researchIds.Contains(h.ResearchId))
                .ToListAsync();

            var compliantCount = 0;
            var totalScreened = histories.Count;
            double totalScreeningHours = 0;

            foreach (var h in histories)
            {
                var r = researches.FirstOrDefault(x => x.Id == h.ResearchId);
                if (r != null)
                {
                    var hours = (h.ChangedAt - r.SubmittedAt).TotalHours;
                    totalScreeningHours += Math.Max(0, hours);
                    if (hours <= slaHours) compliantCount++;
                }
            }

            var avgScreeningHours = totalScreened > 0 ? totalScreeningHours / totalScreened : 0.0;
            var slaCompliance = totalScreened > 0 ? (compliantCount * 100.0 / totalScreened) : 100.0;

            // Dynamic judging time calculation
            var judgingHistories = await _db.ResearchStatusHistories
                .Where(h => h.OldStatus == ResearchStatusConstants.AssignedToJudges &&
                            (h.NewStatus == ResearchStatusConstants.Approved || h.NewStatus == ResearchStatusConstants.ModificationsRequired) &&
                            researchIds.Contains(h.ResearchId))
                .ToListAsync();

            var assignedHistories = await _db.ResearchStatusHistories
                .Where(h => h.NewStatus == ResearchStatusConstants.AssignedToJudges && researchIds.Contains(h.ResearchId))
                .ToListAsync();

            double totalJudgingDays = 0;
            int closedRoundsCount = 0;
            foreach (var jh in judgingHistories)
            {
                var r = researches.FirstOrDefault(x => x.Id == jh.ResearchId);
                if (r != null)
                {
                    var assignTime = assignedHistories
                        .Where(a => a.ResearchId == jh.ResearchId && a.ChangedAt <= jh.ChangedAt)
                        .OrderByDescending(a => a.ChangedAt)
                        .FirstOrDefault()?.ChangedAt ?? r.SubmittedAt;

                    var days = (jh.ChangedAt - assignTime).TotalDays;
                    totalJudgingDays += Math.Max(0, days);
                    closedRoundsCount++;
                }
            }
            var avgJudgingDays = closedRoundsCount > 0 ? totalJudgingDays / closedRoundsCount : 0.0;

            return new SystemStatisticsViewModel
            {
                TotalResearches = total,
                UnderScreeningCount = underScreening,
                AssignedToJudgesCount = assigned,
                ModificationsRequiredCount = modifications,
                ApprovedCount = approved,
                CertificatesIssuedCount = issued,
                TotalResearchers = totalResearchers,
                ResearchesByCategory = byCategory,
                AverageScreeningHours = Math.Round(avgScreeningHours, 1),
                AverageJudgingDays = Math.Round(avgJudgingDays, 1),
                SlaCompliancePercentage = Math.Round(slaCompliance, 1)
            };
        }
    }
}
