using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using researchProject.Data;
using researchProject.Models.Constants;
using researchProject.Models.Entities;
using researchProject.Services.Interfaces;

namespace researchProject.Services.Implementations
{
    public class AttachmentService : IAttachmentService
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileStorageService _fileStorage;
        private readonly UserManager<ApplicationUser> _userManager;
        private static readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

        public AttachmentService(
            ApplicationDbContext db,
            IFileStorageService fileStorage,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _fileStorage = fileStorage;
            _userManager = userManager;
        }

        public async Task<(bool IsAuthorized, bool FileExists, Stream? FileStream, string ContentType, string FileName)> GetAttachmentDownloadAsync(int attachmentId, string userId)
        {
            if (attachmentId <= 0 || string.IsNullOrEmpty(userId))
            {
                return (false, false, null, string.Empty, string.Empty);
            }

            var attachment = await _db.ResearchAttachments
                .Include(a => a.Research)
                .FirstOrDefaultAsync(a => a.Id == attachmentId);

            if (attachment == null || attachment.Research == null)
            {
                return (false, false, null, string.Empty, string.Empty);
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, false, null, string.Empty, string.Empty);
            }

            bool isAuthorized = false;

            // 1. SuperAdmin has universal access
            if (await _userManager.IsInRoleAsync(user, RoleConstants.SuperAdmin))
            {
                isAuthorized = true;
            }

            // 2. College Co-Admin of the matching college
            if (!isAuthorized && await _userManager.IsInRoleAsync(user, RoleConstants.CollegeCoAdmin))
            {
                var coAdmin = await _db.CollegeCoAdmins
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (coAdmin != null && coAdmin.CollegeId == attachment.Research.CollegeId)
                {
                    isAuthorized = true;
                }
            }

            // 3. Assigned Judge for this proposal
            if (!isAuthorized && await _userManager.IsInRoleAsync(user, RoleConstants.Judge))
            {
                var judge = await _db.Judges
                    .AsNoTracking()
                    .FirstOrDefaultAsync(j => j.UserId == userId);

                if (judge != null)
                {
                    var isAssigned = await _db.Evaluations
                        .AsNoTracking()
                        .AnyAsync(e => e.JudgeId == judge.Id && e.ResearchId == attachment.ResearchId);

                    if (isAssigned)
                    {
                        isAuthorized = true;
                    }
                }
            }

            // 4. Research Author (Researcher)
            if (!isAuthorized && await _userManager.IsInRoleAsync(user, RoleConstants.Researcher))
            {
                var isAuthor = await _db.ResearcherResearchesBridge
                    .AsNoTracking()
                    .AnyAsync(b => b.ResearchId == attachment.ResearchId && b.Researcher.UserId == userId);

                if (isAuthor)
                {
                    isAuthorized = true;
                }
            }

            if (!isAuthorized)
            {
                return (false, false, null, string.Empty, string.Empty);
            }

            var subFolder = $"Research_{attachment.ResearchId}";
            var stream = await _fileStorage.GetFileStreamAsync(attachment.StoredFileName, subFolder);

            if (stream == null)
            {
                return (true, false, null, string.Empty, string.Empty);
            }

            if (!_contentTypeProvider.TryGetContentType(attachment.OriginalFileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return (true, true, stream, contentType, attachment.OriginalFileName);
        }
    }
}
