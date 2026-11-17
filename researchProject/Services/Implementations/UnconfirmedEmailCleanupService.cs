using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using researchProject.Data;
using researchProject.Models.Entities;
using researchProject.Services.Interfaces;

namespace researchProject.Services.Implementations
{
    public class UnconfirmedEmailCleanupService : IUnconfirmedEmailCleanupService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UnconfirmedEmailCleanupService> _logger;

        public UnconfirmedEmailCleanupService(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            ILogger<UnconfirmedEmailCleanupService> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<int> CleanupUnconfirmedAccountsAsync()
        {
            var thresholdTime = DateTime.UtcNow.AddHours(-24);
            int deletedCount = 0;

            _logger.LogInformation("بدء مهمة Hangfire لتنظيف الحسابات غير المؤكدة وأكواد التحقق المنتهية. الحد الزمني: {Threshold}", thresholdTime);

            try
            {
                // 1. Identify unconfirmed users with verification codes older than 24 hours
                var unconfirmedCodes = await _db.EmailVerificationCodes
                    .Where(c => c.Purpose == "EmailConfirmation" && !c.IsUsed && c.CreatedAt <= thresholdTime && !string.IsNullOrEmpty(c.UserId))
                    .ToListAsync();

                var targetUserIds = unconfirmedCodes
                    .Select(c => c.UserId!)
                    .Distinct()
                    .ToList();

                foreach (var userId in targetUserIds)
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null && !user.EmailConfirmed)
                    {
                        // Clean up associated Researcher profile if exists
                        var researcher = await _db.Researchers.FirstOrDefaultAsync(r => r.UserId == userId);
                        if (researcher != null)
                        {
                            _db.Researchers.Remove(researcher);
                        }

                        // Clean up all codes for this user
                        var userCodes = await _db.EmailVerificationCodes.Where(c => c.UserId == userId || c.Email == user.Email).ToListAsync();
                        if (userCodes.Any())
                        {
                            _db.EmailVerificationCodes.RemoveRange(userCodes);
                        }

                        await _db.SaveChangesAsync();

                        // Delete ApplicationUser
                        var deleteResult = await _userManager.DeleteAsync(user);
                        if (deleteResult.Succeeded)
                        {
                            deletedCount++;
                            _logger.LogInformation("تم حذف الحساب غير المؤكد بنجاح: {Email} (معرف: {UserId})", user.Email, userId);
                        }
                        else
                        {
                            _logger.LogWarning("تعذر حذف المستخدم {UserId}: {Errors}", userId, string.Join(", ", deleteResult.Errors.Select(e => e.Description)));
                        }
                    }
                }

                // 2. Cleanup expired or used verification codes older than 48 hours
                var oldCodesThreshold = DateTime.UtcNow.AddHours(-48);
                var oldCodes = await _db.EmailVerificationCodes
                    .Where(c => c.ExpiresAt <= oldCodesThreshold || (c.IsUsed && c.CreatedAt <= oldCodesThreshold))
                    .ToListAsync();

                if (oldCodes.Any())
                {
                    _db.EmailVerificationCodes.RemoveRange(oldCodes);
                    await _db.SaveChangesAsync();
                    _logger.LogInformation("تم تنظيف {Count} كود تحقق قديم من قاعدة البيانات.", oldCodes.Count);
                }

                _logger.LogInformation("اكتملت مهمة تنظيف الحسابات غير المؤكدة. إجمالي الحسابات المحذوفة: {Count}", deletedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "حدث خطأ غير متوقع أثناء تنفيذ مهمة تنظيف الحسابات غير المؤكدة في Hangfire.");
            }

            return deletedCount;
        }
    }
}

