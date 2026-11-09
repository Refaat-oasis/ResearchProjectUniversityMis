using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using researchProject.Data;
using researchProject.Models.Constants;
using researchProject.Models.DTOs;
using researchProject.Models.Entities;
using researchProject.Repositories.Interfaces;
using researchProject.Services;
using researchProject.Services.Interfaces;

namespace researchProject.Repositories.Implementations
{
    public class JudgeRepository : IJudgeRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;
        private readonly INotificationService? _notificationService;

        public JudgeRepository(
            ApplicationDbContext db,
            IEmailSender emailSender,
            INotificationService? notificationService = null)
        {
            _db = db;
            _emailSender = emailSender;
            _notificationService = notificationService;
        }

        public async Task<Judge?> GetProfileByUserIdAsync(string userId)
        {
            return await _db.Judges
                .Include(j => j.User)
                .FirstOrDefaultAsync(j => j.UserId == userId);
        }

        public async Task<List<Evaluation>> GetAssignedEvaluationsAsync(string judgeUserId)
        {
            var judge = await _db.Judges.FirstOrDefaultAsync(j => j.UserId == judgeUserId);
            if (judge == null) return new List<Evaluation>();

            // Scoped strictly to researches assigned to this judge
            // AND strictly excluding any research authored by this judge (Conflict of interest)
            return await _db.Evaluations
                .Include(e => e.Research).ThenInclude(r => r.College)
                .Include(e => e.Research).ThenInclude(r => r.Category)
                .Include(e => e.Research).ThenInclude(r => r.ResearchAttachments)
                .Where(e => e.JudgeId == judge.Id &&
                            !_db.ResearcherResearchesBridge.Any(b => b.ResearchId == e.ResearchId && b.Researcher.UserId == judgeUserId))
                .OrderByDescending(e => e.RoundNumber)
                .ThenByDescending(e => e.Id)
                .ToListAsync();
        }

        public async Task<Evaluation?> GetEvaluationDetailsAsync(int evaluationId, string judgeUserId)
        {
            var judge = await _db.Judges.FirstOrDefaultAsync(j => j.UserId == judgeUserId);
            if (judge == null) return null;

            var evaluation = await _db.Evaluations
                .Include(e => e.Research).ThenInclude(r => r.College)
                .Include(e => e.Research).ThenInclude(r => r.Category)
                .Include(e => e.Research).ThenInclude(r => r.ResearchAttachments)
                .FirstOrDefaultAsync(e => e.Id == evaluationId && e.JudgeId == judge.Id);

            if (evaluation == null) return null;

            // Strict conflict of interest check
            bool isAuthor = await _db.ResearcherResearchesBridge
                .AnyAsync(b => b.ResearchId == evaluation.ResearchId && b.Researcher.UserId == judgeUserId);
            if (isAuthor) return null;

            return evaluation;
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> SubmitEvaluationAsync(EvaluationSubmitDto dto, string judgeUserId)
        {
            var judge = await _db.Judges.FirstOrDefaultAsync(j => j.UserId == judgeUserId);
            if (judge == null) return (false, "تعذر التحقق من هوية المحكم.");

            var evaluation = await _db.Evaluations
                .Include(e => e.Research)
                .FirstOrDefaultAsync(e => e.Id == dto.EvaluationId && e.JudgeId == judge.Id);

            if (evaluation == null) return (false, "التقييم المطلوب غير مسند إليك.");

            // Strict conflict of interest check
            bool isAuthor = await _db.ResearcherResearchesBridge
                .AnyAsync(b => b.ResearchId == evaluation.ResearchId && b.Researcher.UserId == judgeUserId);
            if (isAuthor) return (false, "لا يمكن تحكيم بحث أنت أحد مؤلفيه لوجود تعارض مصالح صريح.");

            // Current round check
            var currentMaxRound = await _db.Evaluations
                .Where(e => e.ResearchId == evaluation.ResearchId)
                .MaxAsync(e => e.RoundNumber);

            if (evaluation.RoundNumber != currentMaxRound)
            {
                return (false, "لا يمكن تقديم التقييم لجولة تحكيمية سابقة منتهية.");
            }

            // Check if closed due to other judge
            if (evaluation.Decision == EvaluationDecisionConstants.ClosedDueToOtherJudge)
            {
                return (false, "تم إغلاق التحكيم لهذا المقترح نظراً لاعتماد تقييم محكم آخر.");
            }

            // Check if round is already closed by another judge's evaluation
            var otherAlreadySubmitted = await _db.Evaluations.AnyAsync(e =>
                e.ResearchId == evaluation.ResearchId &&
                e.RoundNumber == currentMaxRound &&
                e.Id != evaluation.Id &&
                e.SubmittedAt.HasValue &&
                e.Decision != EvaluationDecisionConstants.ClosedDueToOtherJudge);

            if (otherAlreadySubmitted)
            {
                return (false, "تم إغلاق التحكيم لهذا المقترح نظراً لاعتماد تقييم محكم آخر.");
            }

            // Already submitted check
            if (evaluation.SubmittedAt.HasValue)
            {
                return (false, "تم تقديم هذا التقييم وتوقيعه رقمياً مسبقاً ولا يمكن تعديله.");
            }

            // Validation for decision constants (BUG-10)
            if (dto.Decision != EvaluationDecisionConstants.Approved && dto.Decision != EvaluationDecisionConstants.ModificationsRequired)
            {
                return (false, "قرار التحكيم غير صالح. يجب اختيار إما (موافق) أو (طلب تعديلات).");
            }

            // Validation for modifications
            if (dto.Decision == EvaluationDecisionConstants.ModificationsRequired && string.IsNullOrWhiteSpace(dto.ModificationNotes))
            {
                return (false, "عند طلب تعديلات، يجب كتابة تفاصيل وملاحظات التعديل المطلوبة من الباحث.");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // BUG-18: Verify proposal status and concurrent submissions inside transaction
                var research = await _db.Researches.FirstOrDefaultAsync(r => r.Id == evaluation.ResearchId);
                if (research == null || research.WorkflowStatus != ResearchStatusConstants.AssignedToJudges)
                {
                    await transaction.RollbackAsync();
                    return (false, "تم إغلاق التحكيم لهذا المقترح نظراً لاعتماد تقييم محكم آخر أو تغير حالة المقترح.");
                }

                var concurrentSubmitted = await _db.Evaluations.AnyAsync(e =>
                    e.ResearchId == evaluation.ResearchId &&
                    e.RoundNumber == currentMaxRound &&
                    e.Id != evaluation.Id &&
                    e.SubmittedAt.HasValue &&
                    e.Decision != EvaluationDecisionConstants.ClosedDueToOtherJudge);

                if (concurrentSubmitted)
                {
                    await transaction.RollbackAsync();
                    return (false, "تم إغلاق التحكيم لهذا المقترح نظراً لاعتماد تقييم محكم آخر.");
                }

                evaluation.PrivacyProtectionRating = dto.PrivacyProtectionRating;
                evaluation.RiskLevel = dto.RiskLevel;
                evaluation.InformedConsentRating = dto.InformedConsentRating;
                evaluation.Recommendations = dto.Recommendations;
                evaluation.ModificationNotes = dto.ModificationNotes;
                evaluation.Decision = dto.Decision;
                evaluation.IsDigitallySigned = true;
                evaluation.SignedAt = DateTime.UtcNow;
                evaluation.SubmittedAt = DateTime.UtcNow;

                // Requirement 11: When more than one judge is assigned, if one made the evaluation, close it for others
                var otherPendingEvaluations = await _db.Evaluations
                    .Where(e => e.ResearchId == evaluation.ResearchId && e.RoundNumber == currentMaxRound && e.Id != evaluation.Id && !e.SubmittedAt.HasValue)
                    .ToListAsync();

                foreach (var other in otherPendingEvaluations)
                {
                    other.Decision = EvaluationDecisionConstants.ClosedDueToOtherJudge;
                    other.ModificationNotes = "تم إغلاق التحكيم لهذا المقترح نظراً لاعتماد تقييم محكم آخر في هذه الجولة.";
                    other.SubmittedAt = DateTime.UtcNow;
                }

                await _db.SaveChangesAsync();

                // Close round immediately based on this judge's decision
                await CloseRoundWithDecisionAsync(evaluation.ResearchId, currentMaxRound, dto.Decision, dto.ModificationNotes, judgeUserId);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"فشل حفظ التقييم: {ex.Message}");
            }
        }

        private async Task CloseRoundWithDecisionAsync(int researchId, int roundNumber, string decision, string? modificationNotes, string actingUserId)
        {
            var research = await _db.Researches
                .Include(r => r.ResearcherResearchesBridges).ThenInclude(b => b.Researcher).ThenInclude(res => res.User)
                .Include(r => r.College).ThenInclude(c => c.CollegeCoAdmins).ThenInclude(a => a.User)
                .FirstOrDefaultAsync(r => r.Id == researchId);

            if (research == null) return;

            var oldStatus = research.WorkflowStatus;

            if (decision == EvaluationDecisionConstants.Approved)
            {
                research.WorkflowStatus = ResearchStatusConstants.Approved;

                var history = new ResearchStatusHistory
                {
                    ResearchId = researchId,
                    OldStatus = oldStatus,
                    NewStatus = ResearchStatusConstants.Approved,
                    ChangedByUserId = actingUserId,
                    ChangedAt = DateTime.UtcNow,
                    Reason = $"اعتماد المقترح بموافقة المحكم في الجولة التحكيمية رقم {roundNumber} وإغلاق التحكيم لبقية المحكمين."
                };
                await _db.ResearchStatusHistories.AddAsync(history);

                // Notify researchers and Co-Admin safely without impacting database transaction
                try
                {
                    foreach (var b in research.ResearcherResearchesBridges)
                    {
                        if (!string.IsNullOrEmpty(b.Researcher?.User?.Email))
                        {
                            _ = _emailSender.SendStatusChangedNotificationAsync(
                                b.Researcher.User.Email,
                                research.Title,
                                oldStatus,
                                ResearchStatusConstants.Approved,
                                "نهنئكم بموافقة المحكم واعتماد المقترح، وهو الآن بانتظار إصدار الشهادة من مدير اللجنة.");
                        }

                        if (_notificationService != null && !string.IsNullOrEmpty(b.Researcher?.UserId))
                        {
                            await _notificationService.SendNotificationAsync(
                                b.Researcher.UserId,
                                "موافقة هيئة التحكيم على المقترح",
                                $"نهنئكم بموافقة المحكم على المقترح '{research.Title}'، وهو بانتظار الاعتماد النهائي وإصدار الشهادة.",
                                $"/Researcher/Tracker/{research.Id}",
                                "StatusChange",
                                "bi-check-all");
                        }
                    }

                    if (_notificationService != null)
                    {
                        foreach (var coAdmin in research.College.CollegeCoAdmins)
                        {
                            if (!string.IsNullOrEmpty(coAdmin.UserId))
                            {
                                await _notificationService.SendNotificationAsync(
                                    coAdmin.UserId,
                                    "مقترح جاهز للاعتماد النهائي",
                                    $"تمت موافقة المحكم على المقترح '{research.Title}'، وأصبح جاهزاً للاعتماد وإصدار الشهادة الأخلاقية.",
                                    "/CollegeCoAdmin/FinalApproval",
                                    "ReadyForApproval",
                                    "bi-award");
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore notification exceptions during evaluation completion
                }
            }
            else
            {
                research.WorkflowStatus = ResearchStatusConstants.ModificationsRequired;

                var history = new ResearchStatusHistory
                {
                    ResearchId = researchId,
                    OldStatus = oldStatus,
                    NewStatus = ResearchStatusConstants.ModificationsRequired,
                    ChangedByUserId = actingUserId,
                    ChangedAt = DateTime.UtcNow,
                    Reason = $"طلب تعديلات من قِبل المحكم في الجولة رقم {roundNumber}:\n{modificationNotes}"
                };
                await _db.ResearchStatusHistories.AddAsync(history);

                try
                {
                    foreach (var b in research.ResearcherResearchesBridges)
                    {
                        if (!string.IsNullOrEmpty(b.Researcher?.User?.Email))
                        {
                            _ = _emailSender.SendStatusChangedNotificationAsync(
                                b.Researcher.User.Email,
                                research.Title,
                                oldStatus,
                                ResearchStatusConstants.ModificationsRequired,
                                $"ملاحظات التعديل المطلوبة من هيئة التحكيم:\n{modificationNotes}");
                        }

                        if (_notificationService != null && !string.IsNullOrEmpty(b.Researcher?.UserId))
                        {
                            await _notificationService.SendNotificationAsync(
                                b.Researcher.UserId,
                                "مطلوب إجراء تعديلات على المقترح",
                                $"طلب المحكم استيفاء تعديلات وملاحظات على مقترحكم '{research.Title}'.",
                                $"/Researcher/Modify/{research.Id}",
                                "ModificationRequired",
                                "bi-pencil-square");
                        }
                    }

                    if (_notificationService != null)
                    {
                        foreach (var coAdmin in research.College.CollegeCoAdmins)
                        {
                            if (!string.IsNullOrEmpty(coAdmin.UserId))
                            {
                                await _notificationService.SendNotificationAsync(
                                    coAdmin.UserId,
                                    "طلب تعديلات على مقترح بحثي",
                                    $"قرر المحكم طلب تعديلات على المقترح: '{research.Title}'. تم إخطار الباحث بذلك.",
                                    $"/CollegeCoAdmin/Review/{research.Id}",
                                    "StatusChange",
                                    "bi-exclamation-circle");
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore notification exceptions during evaluation completion
                }
            }
        }
    }
}
