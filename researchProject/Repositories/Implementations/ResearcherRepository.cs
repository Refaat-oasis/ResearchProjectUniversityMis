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
using researchProject.Services.Interfaces;

namespace researchProject.Repositories.Implementations
{
    public class ResearcherRepository : IResearcherRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileStorageService _fileStorage;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly ICertificateService? _certificateService;
        private static readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg" };

        private static bool CheckAllowedExtension(string fileName)
        {
            var ext = Path.GetExtension(fileName ?? "").ToLowerInvariant();
            return Array.Exists(_allowedExtensions, e => e == ext);
        }

        private async Task<(string StoredFileName, long FileSize, string RelativePath)> SaveAttachmentSafeAsync(byte[] bytes, IFormFile file, string sFolder)
        {
            try
            {
                var res = await _fileStorage.SaveFileBytesAsync(bytes, file.FileName, sFolder);
                if (!string.IsNullOrEmpty(res.StoredFileName))
                    return res;
            }
            catch
            {
            }
            return await _fileStorage.SaveFileAsync(file, sFolder);
        }
        private readonly INotificationService? _notificationService;
        private readonly ILogger<ResearcherRepository>? _logger;

        public ResearcherRepository(
            ApplicationDbContext db,
            IFileStorageService fileStorage,
            IEmailSender emailSender,
            IConfiguration configuration,
            ICertificateService? certificateService = null,
            INotificationService? notificationService = null,
            ILogger<ResearcherRepository>? logger = null)
        {
            _db = db;
            _fileStorage = fileStorage;
            _emailSender = emailSender;
            _configuration = configuration;
            _certificateService = certificateService;
            _notificationService = notificationService;
            _logger = logger;
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
                .Where(c => c.CollegeId == collegeId && c.IsActive)
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

            // BUG-19: Enforce institutional college isolation
            if (dto.CollegeId != researcher.CollegeId)
            {
                return (false, "غير مصرح لك بتقديم مقترح يتبع كلية غير كليتك المسجلة.", null);
            }
            if (category.CollegeId != researcher.CollegeId)
            {
                return (false, "تصنيف البحث المحدد لا يتبع كليتك.", null);
            }

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

            if (dto.InformedConsentFile == null || dto.InformedConsentFile.Length == 0)
            {
                return (false, "ملف وثيقة الموافقة المستنيرة (Informed Consent) فارغ أو لم يتم إرفاقه. يرجى اختيار ملف صالح غير فارغ.", null);
            }
            if (dto.ResearchToolsFile == null || dto.ResearchToolsFile.Length == 0)
            {
                return (false, "ملف أدوات وأجهزة البحث (Research Tools) فارغ أو لم يتم إرفاقه. يرجى اختيار ملف صالح غير فارغ.", null);
            }
            if (dto.ResearchProposalFile == null || dto.ResearchProposalFile.Length == 0)
            {
                return (false, "ملف خطة المقترح البحثي الكاملة (Research Proposal) فارغ أو لم يتم إرفاقه. يرجى اختيار ملف صالح غير فارغ.", null);
            }
            if (dto.AdditionalDocumentFile != null && dto.AdditionalDocumentFile.Length == 0)
            {
                return (false, "الملف الإضافي المرفق فارغ (حجمه 0 بايت). يرجى اختيار ملف صالح أو إلغاء إرفاقه.", null);
            }

            if (!CheckAllowedExtension(dto.InformedConsentFile.FileName))
            {
                return (false, "نوع ملف وثيقة الموافقة المستنيرة غير مسموح به. الصيغ المسموحة: PDF, DOC, DOCX.", null);
            }
            if (!CheckAllowedExtension(dto.ResearchToolsFile.FileName))
            {
                return (false, "نوع ملف أدوات وأجهزة البحث غير مسموح به. الصيغ المسموحة: PDF, DOC, DOCX, XLS, XLSX.", null);
            }
            if (!CheckAllowedExtension(dto.ResearchProposalFile.FileName))
            {
                return (false, "نوع ملف خطة المقترح البحثي غير مسموح به. الصيغ المسموحة: PDF, DOC, DOCX.", null);
            }
            if (dto.AdditionalDocumentFile != null && !CheckAllowedExtension(dto.AdditionalDocumentFile.FileName))
            {
                return (false, "نوع الملف الإضافي غير مسموح به.", null);
            }

            var slaHours = _configuration.GetValue<int>("Screening:SlaHours", 48);
            var now = DateTime.UtcNow;

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

            var savedFiles = new List<(string StoredFileName, string SubFolder)>();

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                await _db.Researches.AddAsync(research);
                await _db.SaveChangesAsync();

                var subFolder = $"Research_{research.Id}";

                // Link author in bridge
                var bridge = new ResearcherResearchesBridge
                {
                    ResearchId = research.Id,
                    ResearcherId = researcher.Id,
                    IsMainAuthor = true
                };
                await _db.ResearcherResearchesBridge.AddAsync(bridge);

                // Save mandatory attachments in DB and backup to storage outside wwwroot
                // 1. Informed Consent
                byte[] consentBytes;
                using (var ms = new MemoryStream())
                {
                    await dto.InformedConsentFile.CopyToAsync(ms);
                    consentBytes = ms.ToArray();
                }
                var (storedConsent, consentSize, consentPath) = await SaveAttachmentSafeAsync(consentBytes, dto.InformedConsentFile, subFolder);
                savedFiles.Add((storedConsent, subFolder));
                var attConsent = new ResearchAttachment
                {
                    ResearchId = research.Id,
                    AttachmentType = AttachmentTypeConstants.InformedConsent,
                    Version = 1,
                    OriginalFileName = dto.InformedConsentFile.FileName,
                    StoredFileName = storedConsent,
                    FilePath = consentPath,
                    FileSize = consentSize,
                    ContentType = dto.InformedConsentFile.ContentType,
                    UploadedAt = now
                };
                await _db.ResearchAttachments.AddAsync(attConsent);

                // 2. Research Tools
                byte[] toolsBytes;
                using (var ms = new MemoryStream())
                {
                    await dto.ResearchToolsFile.CopyToAsync(ms);
                    toolsBytes = ms.ToArray();
                }
                var (storedTools, toolsSize, toolsPath) = await SaveAttachmentSafeAsync(toolsBytes, dto.ResearchToolsFile, subFolder);
                savedFiles.Add((storedTools, subFolder));
                var attTools = new ResearchAttachment
                {
                    ResearchId = research.Id,
                    AttachmentType = AttachmentTypeConstants.ResearchTools,
                    Version = 1,
                    OriginalFileName = dto.ResearchToolsFile.FileName,
                    StoredFileName = storedTools,
                    FilePath = toolsPath,
                    FileSize = toolsSize,
                    ContentType = dto.ResearchToolsFile.ContentType,
                    UploadedAt = now
                };
                await _db.ResearchAttachments.AddAsync(attTools);

                // 3. Research Proposal
                byte[] propBytes;
                using (var ms = new MemoryStream())
                {
                    await dto.ResearchProposalFile.CopyToAsync(ms);
                    propBytes = ms.ToArray();
                }
                var (storedProp, propSize, propPath) = await SaveAttachmentSafeAsync(propBytes, dto.ResearchProposalFile, subFolder);
                savedFiles.Add((storedProp, subFolder));
                var attProp = new ResearchAttachment
                {
                    ResearchId = research.Id,
                    AttachmentType = AttachmentTypeConstants.ResearchProposal,
                    Version = 1,
                    OriginalFileName = dto.ResearchProposalFile.FileName,
                    StoredFileName = storedProp,
                    FilePath = propPath,
                    FileSize = propSize,
                    ContentType = dto.ResearchProposalFile.ContentType,
                    UploadedAt = now
                };
                await _db.ResearchAttachments.AddAsync(attProp);

                // 4. Additional Document (Optional)
                if (dto.AdditionalDocumentFile != null && dto.AdditionalDocumentFile.Length > 0)
                {
                    byte[] addBytes;
                    using (var ms = new MemoryStream())
                    {
                        await dto.AdditionalDocumentFile.CopyToAsync(ms);
                        addBytes = ms.ToArray();
                    }
                    var (storedAdd, addSize, addPath) = await SaveAttachmentSafeAsync(addBytes, dto.AdditionalDocumentFile, subFolder);
                    savedFiles.Add((storedAdd, subFolder));
                    var attAdd = new ResearchAttachment
                    {
                        ResearchId = research.Id,
                        AttachmentType = AttachmentTypeConstants.AdditionalDocument,
                        Version = 1,
                        OriginalFileName = dto.AdditionalDocumentFile.FileName,
                        StoredFileName = storedAdd,
                        FilePath = addPath,
                        FileSize = addSize,
                        ContentType = dto.AdditionalDocumentFile.ContentType,
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
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                foreach (var (fName, sFolder) in savedFiles)
                {
                    try
                    {
                        _fileStorage.DeleteFile(fName, sFolder);
                    }
                    catch
                    {
                        // Ignore cleanup failures during rollback
                    }
                }
                return (false, $"فشل تقديم المقترح البحثي: {ex.Message}", null);
            }

            // Post-commit notifications (isolated from database transaction)
            try
            {
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

                // Trigger in-app SignalR notifications
                if (_notificationService != null)
                {
                    // 1. Notify Researcher confirmation
                    await _notificationService.SendNotificationAsync(
                        researcherUserId,
                        "تم تقديم المقترح بنجاح",
                        $"تم استلام مقترحكم '{research.Title}' وبدأت مرحلة الفرز الإداري بالكلية.",
                        $"/Researcher/Tracker/{research.Id}",
                        "NewSubmission",
                        "bi-check2-circle");

                    // 2. Notify College Co-Admins of the college
                    var coAdmins = await _db.CollegeCoAdmins
                        .Where(a => a.CollegeId == dto.CollegeId)
                        .Select(a => a.UserId)
                        .ToListAsync();

                    foreach (var coAdminUserId in coAdmins)
                    {
                        await _notificationService.SendNotificationAsync(
                            coAdminUserId,
                            "مقترح بحثي جديد يتطلب الفرز",
                            $"تم تقديم مقترح بحثي جديد: '{research.Title}' ينتظر مراجعة الفرز الإداري وتعيين المحكمين.",
                            $"/CollegeCoAdmin/Review/{research.Id}",
                            "NewSubmission",
                            "bi-file-earmark-plus");
                    }
                }
            }
            catch (Exception notifEx)
            {
                _logger?.LogWarning(notifEx, "Failed to send notifications after creating research proposal {ResearchId}", research.Id);
            }

            return (true, null, research.Id);
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
            var oldStatus = research.WorkflowStatus;
            int nextRound = 1;
            List<Judge> activeJudges = new();

            if (dto.ModifiedProposalFile == null || dto.ModifiedProposalFile.Length == 0)
            {
                return (false, "ملف المقترح البحثي المعدل فارغ أو لم يتم إرفاقه. يرجى اختيار ملف صالح غير فارغ.");
            }

            // Determine latest version of attachments
            var maxVersion = await _db.ResearchAttachments
                .Where(a => a.ResearchId == research.Id)
                .MaxAsync(a => (int?)a.Version) ?? 1;

            var newVersion = maxVersion + 1;

            using var transaction = await _db.Database.BeginTransactionAsync();
            var savedModifiedFiles = new List<(string StoredFileName, string SubFolder)>();
            try
            {
                // Save modified proposal file in DB and backup to storage
                byte[] modPropBytes;
                using (var ms = new MemoryStream())
                {
                    await dto.ModifiedProposalFile.CopyToAsync(ms);
                    modPropBytes = ms.ToArray();
                }
                var (storedProposal, propSize, proposalPath) = await SaveAttachmentSafeAsync(modPropBytes, dto.ModifiedProposalFile, subFolder);
                savedModifiedFiles.Add((storedProposal, subFolder));
                var attProposal = new ResearchAttachment
                {
                    ResearchId = research.Id,
                    AttachmentType = AttachmentTypeConstants.ModifiedFile,
                    Version = newVersion,
                    OriginalFileName = dto.ModifiedProposalFile.FileName,
                    StoredFileName = storedProposal,
                    FilePath = proposalPath,
                    FileSize = propSize,
                    ContentType = dto.ModifiedProposalFile.ContentType,
                    UploadedAt = now
                };
                await _db.ResearchAttachments.AddAsync(attProposal);

                if (dto.ModifiedToolsFile != null && dto.ModifiedToolsFile.Length > 0)
                {
                    byte[] modToolsBytes;
                    using (var ms = new MemoryStream())
                    {
                        await dto.ModifiedToolsFile.CopyToAsync(ms);
                        modToolsBytes = ms.ToArray();
                    }
                    var (storedTools, toolsSize, toolsPath) = await SaveAttachmentSafeAsync(modToolsBytes, dto.ModifiedToolsFile, subFolder);
                    savedModifiedFiles.Add((storedTools, subFolder));
                    var attTools = new ResearchAttachment
                    {
                        ResearchId = research.Id,
                        AttachmentType = AttachmentTypeConstants.ResearchTools,
                        Version = newVersion,
                        OriginalFileName = dto.ModifiedToolsFile.FileName,
                        StoredFileName = storedTools,
                        FilePath = toolsPath,
                        FileSize = toolsSize,
                        ContentType = dto.ModifiedToolsFile.ContentType,
                        UploadedAt = now
                    };
                    await _db.ResearchAttachments.AddAsync(attTools);
                }

                // Determine next round number
                var maxRound = await _db.Evaluations
                    .Where(e => e.ResearchId == research.Id)
                    .MaxAsync(e => (int?)e.RoundNumber) ?? 1;

                nextRound = maxRound + 1;

                // Previous judges from latest round
                var previousJudgeIds = await _db.Evaluations
                    .Where(e => e.ResearchId == research.Id && e.RoundNumber == maxRound)
                    .Select(e => e.JudgeId)
                    .Distinct()
                    .ToListAsync();

                activeJudges = await _db.Judges
                    .Include(j => j.User)
                    .Where(j => previousJudgeIds.Contains(j.Id) && j.IsActive)
                    .ToListAsync();

                if (activeJudges.Any())
                {
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
                }
                else
                {
                    // BUG-03 fix: All previous judges became inactive; return to UnderFormalScreening so CoAdmin re-assigns active judges
                    research.WorkflowStatus = ResearchStatusConstants.UnderFormalScreening;

                    var history = new ResearchStatusHistory
                    {
                        ResearchId = research.Id,
                        OldStatus = oldStatus,
                        NewStatus = ResearchStatusConstants.UnderFormalScreening,
                        ChangedByUserId = researcherUserId,
                        ChangedAt = now,
                        Reason = $"رفع المستندات المعدلة من الباحث. ونظراً لعدم توفر المحكمين السابقين، تمت إحالة المقترح للمراجعة الشكلية لإعادة التوزيع على محكمين نشطين.\n{dto.ResearcherNotes}"
                    };
                    await _db.ResearchStatusHistories.AddAsync(history);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                foreach (var (fName, sFolder) in savedModifiedFiles)
                {
                    try
                    {
                        _fileStorage.DeleteFile(fName, sFolder);
                    }
                    catch
                    {
                        // Ignore cleanup failures during rollback
                    }
                }
                return (false, $"فشل رفع التعديلات: {ex.Message}");
            }

            // Post-commit notifications (isolated from database transaction)
            try
            {
                if (research.WorkflowStatus == ResearchStatusConstants.AssignedToJudges)
                {
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

                        if (_notificationService != null && !string.IsNullOrEmpty(judge.UserId))
                        {
                            var judgeEval = await _db.Evaluations
                                .FirstOrDefaultAsync(e => e.ResearchId == research.Id && e.JudgeId == judge.Id && e.RoundNumber == nextRound);

                            var evalTargetUrl = judgeEval != null ? $"/Judge/Evaluate/{judgeEval.Id}" : "/Judge/AssignedResearches";

                            await _notificationService.SendNotificationAsync(
                                judge.UserId,
                                "تم رفع التعديلات وبدء جولة تحكيمية",
                                $"قام الباحث برفع النسخة المعدلة للمقترح: '{research.Title}'. يرجى استكمال التحكيم للجولة {nextRound}.",
                                evalTargetUrl,
                                "JudgeAssignment",
                                "bi-arrow-repeat");
                        }
                    }
                }

                if (_notificationService != null)
                {
                    var coAdmins = await _db.CollegeCoAdmins
                        .Where(a => a.CollegeId == research.CollegeId)
                        .Select(a => a.UserId)
                        .ToListAsync();

                    var notifMsg = research.WorkflowStatus == ResearchStatusConstants.AssignedToJudges
                        ? $"قام الباحث برفع التعديلات للمقترح: '{research.Title}'. بدأت الجولة التحكيمية رقم {nextRound}."
                        : $"قام الباحث برفع التعديلات للمقترح: '{research.Title}'. يتطلب إسناد محكمين نشطين بالكلية.";

                    foreach (var coAdminUserId in coAdmins)
                    {
                        await _notificationService.SendNotificationAsync(
                            coAdminUserId,
                            "استيفاء التعديلات من الباحث",
                            notifMsg,
                            $"/CollegeCoAdmin/Review/{research.Id}",
                            "ModificationUploaded",
                            "bi-arrow-repeat");
                    }
                }
            }
            catch (Exception notifEx)
            {
                _logger?.LogWarning(notifEx, "Failed to send notifications after submitting modifications for research {ResearchId}", research.Id);
            }

            return (true, null);
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
