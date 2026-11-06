using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using researchProject.Data;
using researchProject.Models.Constants;
using researchProject.Models.DTOs;
using researchProject.Models.Entities;
using researchProject.Repositories.Interfaces;
using researchProject.Services;

namespace researchProject.Repositories.Implementations
{
    public class ResearcherRepository : IResearcherRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileStorageService _fileStorage;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly ICertificateService? _certificateService;

        public ResearcherRepository(
            ApplicationDbContext db,
            IFileStorageService fileStorage,
            IEmailSender emailSender,
            IConfiguration configuration,
            ICertificateService? certificateService = null)
        {
            _db = db;
            _fileStorage = fileStorage;
            _emailSender = emailSender;
            _configuration = configuration;
            _certificateService = certificateService;
        }

        public async Task<Researcher?> GetProfileByUserIdAsync(string userId)
        {
            return await _db.Researchers
                .Include(r => r.College)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserId == userId);
        }

        public async Task<List<ResearchCategory>> GetCategoriesForCollegeAsync(int collegeId)
        {
            return await _db.ResearchCategories
                .Where(c => c.CollegeId == collegeId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<ResearchCategory?> GetCategoryWithFieldsAsync(int categoryId)
        {
            return await _db.ResearchCategories
                .Include(c => c.College)
                .FirstOrDefaultAsync(c => c.Id == categoryId);
        }

        public async Task<List<Research>> GetMyResearchesAsync(string researcherUserId)
        {
            return await _db.Researches
                .Include(r => r.College)
                .Include(r => r.Category)
                .Include(r => r.ResearcherResearchesBridges)
                .Where(r => r.ResearcherResearchesBridges.Any(b => b.Researcher.UserId == researcherUserId))
                .OrderByDescending(r => r.SubmittedAt)
                .ToListAsync();
        }

        public async Task<Research?> GetResearchDetailsAsync(int researchId, string researcherUserId)
        {
            return await _db.Researches
                .Include(r => r.College)
                .Include(r => r.Category)
                .Include(r => r.ResearchAttachments)
                .Include(r => r.ResearcherResearchesBridges).ThenInclude(b => b.Researcher).ThenInclude(res => res.User)
                .Include(r => r.ResearchStatusHistories).ThenInclude(h => h.ChangedByUser)
                .FirstOrDefaultAsync(r => r.Id == researchId &&
                                         r.ResearcherResearchesBridges.Any(b => b.Researcher.UserId == researcherUserId));
        }

        public async Task<(bool Succeeded, string? ErrorMessage, int? ResearchId)> CreateResearchWithAttachmentsAsync(ResearchCreateDto dto, string researcherUserId)
        {
            var researcher = await _db.Researchers
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserId == researcherUserId);

            if (researcher == null) return (false, "تعذر العثور على الملف التعريفي للباحث.", null);

            var category = await _db.ResearchCategories.FindAsync(dto.CategoryId);
            if (category == null) return (false, "تصنيف البحث غير موجود.", null);

            // Server-side validation of dynamic category fields
            if (!string.IsNullOrEmpty(category.FieldsDefinitionJson))
            {
                try
                {
                    using var doc = JsonDocument.Parse(category.FieldsDefinitionJson);
                    foreach (var element in doc.RootElement.EnumerateArray())
                    {
                        var fieldName = element.GetProperty("Name").GetString() ?? "";
                        var fieldLabel = element.TryGetProperty("Label", out var labelProp) ? labelProp.GetString() : fieldName;
                        var isRequired = element.TryGetProperty("IsRequired", out var reqProp) && reqProp.GetBoolean();

                        if (isRequired)
                        {
                            if (!dto.DynamicAnswers.TryGetValue(fieldName, out var answer) || string.IsNullOrWhiteSpace(answer))
                            {
                                return (false, $"الحقل الإضافي المطلوب ({fieldLabel}) إلزامي في هذا التصنيف.", null);
                            }
                        }
                    }
                }
                catch
                {
                    // If JSON was invalid, ignore dynamic field syntax error
                }
            }

            var slaHours = _configuration.GetValue<int>("Screening:SlaHours", 48);
            var now = DateTime.UtcNow;

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var research = new Research
                {
                    Title = dto.Title,
                    CollegeId = dto.CollegeId,
                    CategoryId = dto.CategoryId,
                    AcademicSpecialty = dto.AcademicSpecialty,
                    DynamicFieldsJson = JsonSerializer.Serialize(dto.DynamicAnswers),
                    WorkflowStatus = ResearchStatusConstants.UnderFormalScreening,
                    SubmittedAt = now,
                    ScreeningDueAt = now.AddHours(slaHours)
                };

                await _db.Researches.AddAsync(research);
                await _db.SaveChangesAsync();

                // Link author in bridge
                var bridge = new ResearcherResearchesBridge
                {
                    ResearchId = research.Id,
                    ResearcherId = researcher.Id,
                    IsMainAuthor = true
                };
                await _db.ResearcherResearchesBridge.AddAsync(bridge);

                // Save mandatory attachments outside wwwroot
                var subFolder = $"Research_{research.Id}";

                // 1. Informed Consent
                var (storedConsent, consentSize) = await _fileStorage.SaveFileAsync(dto.InformedConsentFile, subFolder);
                var attConsent = new ResearchAttachment
                {
                    ResearchId = research.Id,
                    AttachmentType = AttachmentTypeConstants.InformedConsent,
                    Version = 1,
                    OriginalFileName = dto.InformedConsentFile.FileName,
                    StoredFileName = storedConsent,
                    FileSize = consentSize,
                    UploadedAt = now
                };
                await _db.ResearchAttachments.AddAsync(attConsent);

                // 2. Research Tools
                var (storedTools, toolsSize) = await _fileStorage.SaveFileAsync(dto.ResearchToolsFile, subFolder);
                var attTools = new ResearchAttachment
                {
                    ResearchId = research.Id,
                    AttachmentType = AttachmentTypeConstants.ResearchTools,
                    Version = 1,
                    OriginalFileName = dto.ResearchToolsFile.FileName,
                    StoredFileName = storedTools,
                    FileSize = toolsSize,
                    UploadedAt = now
                };
                await _db.ResearchAttachments.AddAsync(attTools);

                // 3. Research Proposal
                var (storedProp, propSize) = await _fileStorage.SaveFileAsync(dto.ResearchProposalFile, subFolder);
                var attProp = new ResearchAttachment
                {
                    ResearchId = research.Id,
                    AttachmentType = AttachmentTypeConstants.ResearchProposal,
                    Version = 1,
                    OriginalFileName = dto.ResearchProposalFile.FileName,
                    StoredFileName = storedProp,
                    FileSize = propSize,
                    UploadedAt = now
                };
                await _db.ResearchAttachments.AddAsync(attProp);

                // 4. Additional Document (Optional)
                if (dto.AdditionalDocumentFile != null && dto.AdditionalDocumentFile.Length > 0)
                {
                    var (storedAdd, addSize) = await _fileStorage.SaveFileAsync(dto.AdditionalDocumentFile, subFolder);
                    var attAdd = new ResearchAttachment
                    {
                        ResearchId = research.Id,
                        AttachmentType = AttachmentTypeConstants.AdditionalDocument,
                        Version = 1,
                        OriginalFileName = dto.AdditionalDocumentFile.FileName,
                        StoredFileName = storedAdd,
                        FileSize = addSize,
                        UploadedAt = now
                    };
                    await _db.ResearchAttachments.AddAsync(attAdd);
                }

                // Status history
                var history = new ResearchStatusHistory
                {
                    ResearchId = research.Id,
                    OldStatus = null,
                    NewStatus = ResearchStatusConstants.UnderFormalScreening,
                    ChangedByUserId = researcherUserId,
                    ChangedAt = now,
                    Reason = "تقديم المقترح البحثي ورفع الوثائق المرفقة والموافقة المستنيرة لأول مرة."
                };
                await _db.ResearchStatusHistories.AddAsync(history);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                // Send email receipt to researcher
                if (!string.IsNullOrEmpty(researcher.User.Email))
                {
                    _ = _emailSender.SendStatusChangedNotificationAsync(
                        researcher.User.Email,
                        research.Title,
                        "طلب جديد",
                        ResearchStatusConstants.UnderFormalScreening,
                        $"تم استلام المقترح البحثي بنجاح، وهو قيد المراجعة الشكلية من إدارة اللجنة (الموعد المستهدف: خلال {slaHours} ساعة).");
                }

                return (true, null, research.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"فشل تقديم المقترح البحثي: {ex.Message}", null);
            }
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> SubmitModifiedAttachmentsAsync(ResearchModificationDto dto, string researcherUserId)
        {
            var research = await _db.Researches
                .Include(r => r.ResearcherResearchesBridges).ThenInclude(b => b.Researcher)
                .Include(r => r.Evaluations).ThenInclude(e => e.Judge)
                .FirstOrDefaultAsync(r => r.Id == dto.ResearchId);

            if (research == null) return (false, "المقترح البحثي غير موجود.");

            // Ownership check: must be an author
            bool isAuthor = research.ResearcherResearchesBridges.Any(b => b.Researcher.UserId == researcherUserId);
            if (!isAuthor) return (false, "غير مصرح لك بإجراء تعديلات على هذا المقترح البحثي.");

            // Status check: must be in ModificationsRequired status
            if (research.WorkflowStatus != ResearchStatusConstants.ModificationsRequired)
            {
                return (false, "لا يمكن رفع تعديلات إلا عندما تكون حالة البحث (تعديلات مطلوبة).");
            }

            var now = DateTime.UtcNow;
            var subFolder = $"Research_{research.Id}";

            // Determine latest version of attachments
            var maxVersion = await _db.ResearchAttachments
                .Where(a => a.ResearchId == research.Id)
                .MaxAsync(a => (int?)a.Version) ?? 1;

            var newVersion = maxVersion + 1;

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Save modified proposal file
                var (storedProposal, propSize) = await _fileStorage.SaveFileAsync(dto.ModifiedProposalFile, subFolder);
                var attProposal = new ResearchAttachment
                {
                    ResearchId = research.Id,
                    AttachmentType = AttachmentTypeConstants.ModifiedFile,
                    Version = newVersion,
                    OriginalFileName = dto.ModifiedProposalFile.FileName,
                    StoredFileName = storedProposal,
                    FileSize = propSize,
                    UploadedAt = now
                };
                await _db.ResearchAttachments.AddAsync(attProposal);

                if (dto.ModifiedToolsFile != null && dto.ModifiedToolsFile.Length > 0)
                {
                    var (storedTools, toolsSize) = await _fileStorage.SaveFileAsync(dto.ModifiedToolsFile, subFolder);
                    var attTools = new ResearchAttachment
                    {
                        ResearchId = research.Id,
                        AttachmentType = AttachmentTypeConstants.ResearchTools,
                        Version = newVersion,
                        OriginalFileName = dto.ModifiedToolsFile.FileName,
                        StoredFileName = storedTools,
                        FileSize = toolsSize,
                        UploadedAt = now
                    };
                    await _db.ResearchAttachments.AddAsync(attTools);
                }

                // Determine next round number
                var maxRound = await _db.Evaluations
                    .Where(e => e.ResearchId == research.Id)
                    .MaxAsync(e => (int?)e.RoundNumber) ?? 1;

                var nextRound = maxRound + 1;

                // Previous judges from latest round
                var previousJudgeIds = await _db.Evaluations
                    .Where(e => e.ResearchId == research.Id && e.RoundNumber == maxRound)
                    .Select(e => e.JudgeId)
                    .Distinct()
                    .ToListAsync();

                var activeJudges = await _db.Judges
                    .Include(j => j.User)
                    .Where(j => previousJudgeIds.Contains(j.Id) && j.IsActive)
                    .ToListAsync();

                foreach (var judge in activeJudges)
                {
                    var eval = new Evaluation
                    {
                        ResearchId = research.Id,
                        JudgeId = judge.Id,
                        RoundNumber = nextRound,
                        Decision = EvaluationDecisionConstants.UnderEvaluation,
                        IsDigitallySigned = false
                    };
                    await _db.Evaluations.AddAsync(eval);
                }

                var oldStatus = research.WorkflowStatus;
                research.WorkflowStatus = ResearchStatusConstants.AssignedToJudges;

                var history = new ResearchStatusHistory
                {
                    ResearchId = research.Id,
                    OldStatus = oldStatus,
                    NewStatus = ResearchStatusConstants.AssignedToJudges,
                    ChangedByUserId = researcherUserId,
                    ChangedAt = now,
                    Reason = $"رفع المستندات المعدلة من الباحث وبدء جولة تحكيمية جديدة (الجولة {nextRound}).\n{dto.ResearcherNotes}"
                };
                await _db.ResearchStatusHistories.AddAsync(history);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                // Notify judges
                foreach (var judge in activeJudges)
                {
                    if (!string.IsNullOrEmpty(judge.User?.Email))
                    {
                        _ = _emailSender.SendStatusChangedNotificationAsync(
                            judge.User.Email,
                            research.Title,
                            oldStatus,
                            ResearchStatusConstants.AssignedToJudges,
                            $"قام الباحث برفع النسخة المعدلة من المقترح البحثي، وبدأت الجولة التحكيمية رقم {nextRound}.");
                    }
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"فشل تسليم التعديلات: {ex.Message}");
            }
        }

        public async Task<List<Research>> GetMyCertificatesAsync(string researcherUserId)
        {
            return await _db.Researches
                .Include(r => r.College)
                .Include(r => r.Category)
                .Include(r => r.ResearcherResearchesBridges)
                .Where(r => r.WorkflowStatus == ResearchStatusConstants.CertificateIssued &&
                            r.ResearcherResearchesBridges.Any(b => b.Researcher.UserId == researcherUserId))
                .OrderByDescending(r => r.CertificateIssuedAt)
                .ToListAsync();
        }

        public async Task<(byte[]? FileBytes, string? SerialNumber)> GetCertificateDownloadAsync(int researchId, string researcherUserId)
        {
            var research = await GetResearchDetailsAsync(researchId, researcherUserId);
            if (research == null || string.IsNullOrEmpty(research.CertificateFilePath))
            {
                return (null, null);
            }

            if (_certificateService != null)
            {
                var bytes = await _certificateService.GetCertificateFileBytesAsync(research.CertificateFilePath);
                return (bytes, research.CertificateSerialNumber);
            }

            return (null, research.CertificateSerialNumber);
        }
    }
}
