using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using researchProject.Data;
using researchProject.Models.Entities;
using researchProject.Services.Interfaces;

namespace researchProject.Services
{
    public class HangfireEmailDispatcher : IHangfireEmailDispatcher
    {
        private readonly IBackgroundJobClient _backgroundJobs;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<HangfireEmailDispatcher> _logger;

        public HangfireEmailDispatcher(
            IBackgroundJobClient backgroundJobs,
            IServiceScopeFactory scopeFactory,
            ILogger<HangfireEmailDispatcher> logger)
        {
            _backgroundJobs = backgroundJobs;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public string EnqueueJudgeInvitationEmail(
            int invitationId,
            string recipientEmail,
            string recipientName,
            string invitationLink,
            string inviterUserId)
        {
            var subject = "دعوة للانضمام كمحكم في منصة أخلاقيات البحث العلمي";
            var body = $@"
                <div dir='rtl' style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                    <h2 style='color: #1e3a8a; text-align: center;'>منصة أخلاقيات البحث العلمي</h2>
                    <hr style='border: none; border-top: 1px solid #e0e0e0; margin: 20px 0;' />
                    <p>سعادة المحكم: <strong>{recipientName}</strong> المحترم،</p>
                    <p>يسرنا دعوتكم للانضمام إلى منصة أخلاقيات البحث العلمي والمشاركة في تحكيم الأبحاث العلمية المحالة إلى كليتكم.</p>
                    <p>لإكمال التسجيل وقبول الدعوة، يرجى الضغط على الرابط التالي:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{invitationLink}' style='background-color: #2563eb; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;'>قبول الدعوة وتفعيل الحساب</a>
                    </div>
                    <p style='color: #64748b; font-size: 13px;'>ينتهي رابط الدعوة خلال 72 ساعة من وقت إرساله.</p>
                    <hr style='border: none; border-top: 1px solid #e0e0e0; margin: 20px 0;' />
                    <p style='color: #94a3b8; font-size: 12px; text-align: center;'>هذه رسالة آلية، يرجى عدم الرد عليها مباشرة.</p>
                </div>";

            return _backgroundJobs.Enqueue<IHangfireEmailDispatcher>(x =>
                x.SendEmailWithRollbackAsync(recipientEmail, subject, body, EmailJobType.JudgeInvitation, invitationId.ToString(), inviterUserId, 1));
        }

        public string EnqueueCoAdminCredentialsEmail(
            string userId,
            string recipientEmail,
            string fullName,
            string temporaryPassword,
            string collegeName,
            string creatorUserId)
        {
            var subject = "بيانات حسابك كمشرف كلية - منصة أخلاقيات البحث العلمي";
            var body = $@"
                <div dir='rtl' style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                    <h2 style='color: #1e3a8a; text-align: center;'>منصة أخلاقيات البحث العلمي</h2>
                    <hr style='border: none; border-top: 1px solid #e0e0e0; margin: 20px 0;' />
                    <p>مرحباً بك: <strong>{fullName}</strong> المحترم،</p>
                    <p>تم إنشاء حسابك بنجاح كمشرف للجنة الأخلاقيات في <strong>{collegeName}</strong>.</p>
                    <div style='background-color: #f8fafc; padding: 15px; border-radius: 6px; border: 1px solid #cbd5e1; margin: 20px 0;'>
                        <p style='margin: 5px 0;'><strong>اسم المستخدم (البريد الإلكتروني):</strong> {recipientEmail}</p>
                        <p style='margin: 5px 0;'><strong>كلمة المرور المؤقتة:</strong> <span style='font-family: monospace; background: #e2e8f0; padding: 2px 6px; border-radius: 4px; font-weight: bold;'>{temporaryPassword}</span></p>
                    </div>
                    <p style='color: #b91c1c; font-size: 14px;'><strong>ملاحظة هامة:</strong> يرجى تسجيل الدخول فوراً وتغيير كلمة المرور المؤقتة للحفاظ على أمان حسابك.</p>
                    <hr style='border: none; border-top: 1px solid #e0e0e0; margin: 20px 0;' />
                    <p style='color: #94a3b8; font-size: 12px; text-align: center;'>هذه رسالة آلية، يرجى عدم الرد عليها مباشرة.</p>
                </div>";

            return _backgroundJobs.Enqueue<IHangfireEmailDispatcher>(x =>
                x.SendEmailWithRollbackAsync(recipientEmail, subject, body, EmailJobType.CoAdminCredentials, userId, creatorUserId, 1));
        }

        [AutomaticRetry(Attempts = 0)] // We manage our own explicit 2-attempt policy + rollback
        public async Task SendEmailWithRollbackAsync(
            string toEmail,
            string subject,
            string htmlMessage,
            EmailJobType jobType,
            string entityKey,
            string initiatorUserId,
            int attemptNumber)
        {
            using var scope = _scopeFactory.CreateScope();
            var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            try
            {
                _logger.LogInformation("بدء محاولة إرسال بريد رقم {Attempt} إلى {Email} لنوع العملية {JobType}", attemptNumber, toEmail, jobType);

                await emailSender.SendEmailAsync(toEmail, subject, htmlMessage);

                _logger.LogInformation("تم إرسال البريد بنجاح في المحاولة رقم {Attempt} إلى {Email}", attemptNumber, toEmail);

                var initiatorUser = !string.IsNullOrEmpty(initiatorUserId) ? await userManager.FindByIdAsync(initiatorUserId) : null;
                var initiatorRole = initiatorUser != null ? (await userManager.GetRolesAsync(initiatorUser)).FirstOrDefault() : null;

                // Audit log successful delivery
                db.AuditLogs.Add(new AuditLog
                {
                    UserId = initiatorUserId,
                    UserEmail = initiatorUser?.Email ?? toEmail,
                    UserName = initiatorUser?.FullName,
                    UserRole = initiatorRole,
                    ControllerName = "Hangfire",
                    ActionName = $"SendEmail_{jobType}",
                    HttpMethod = "BACKGROUND",
                    Path = $"/hangfire/jobs/{jobType}/{entityKey}",
                    StatusCode = 200,
                    IsSuccess = true,
                    Description = $"تم إرسال بريد {jobType} بنجاح إلى المستلم ({toEmail}) في المحاولة رقم {attemptNumber}.",
                    Timestamp = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "فشلت المحاولة رقم {Attempt} لإرسال البريد إلى {Email} لنوع العملية {JobType}", attemptNumber, toEmail, jobType);

                if (attemptNumber < 2)
                {
                    // Schedule 2nd and final attempt after 15 seconds
                    _logger.LogInformation("جدولة المحاولة الثانية والأخيرة لإرسال البريد إلى {Email} بعد 15 ثانية", toEmail);
                    _backgroundJobs.Schedule<IHangfireEmailDispatcher>(
                        x => x.SendEmailWithRollbackAsync(toEmail, subject, htmlMessage, jobType, entityKey, initiatorUserId, 2),
                        TimeSpan.FromSeconds(15));
                }
                else
                {
                    // 2nd attempt failed! Execute rollback and data deletion
                    _logger.LogError(ex, "فشلت المحاولة الثانية لإرسال البريد إلى {Email}. جاري التراجع وحذف البيانات المرتبطة من قاعدة البيانات وإشعار المستخدم.", toEmail);
                    await ExecuteRollbackAsync(db, userManager, notificationService, jobType, entityKey, toEmail, initiatorUserId, ex.Message);
                }
            }
        }

        private async Task ExecuteRollbackAsync(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService,
            EmailJobType jobType,
            string entityKey,
            string toEmail,
            string initiatorUserId,
            string errorMessage)
        {
            string rollbackMessage = string.Empty;

            if (jobType == EmailJobType.JudgeInvitation)
            {
                if (int.TryParse(entityKey, out var invitationId))
                {
                    var invitation = await db.JudgeInvitations.FirstOrDefaultAsync(i => i.Id == invitationId);
                    if (invitation != null)
                    {
                        db.JudgeInvitations.Remove(invitation);
                        await db.SaveChangesAsync();
                        _logger.LogInformation("تم حذف سجل دعوة التحكيم رقم {InvitationId} للمحكم {Email} بسبب فشل إرسال البريد بعد محاولتين.", invitationId, toEmail);
                    }
                }

                rollbackMessage = $"تعذر إرسال بريد دعوة التحكيم إلى ({toEmail}) بعد محاولتين. تم إلغاء الدعوة وحذفها تلقائياً من النظام، يرجى التحقق من صحة البريد وإعادة المحاولة.";
            }
            else if (jobType == EmailJobType.CoAdminCredentials)
            {
                var coAdmin = await db.CollegeCoAdmins.FirstOrDefaultAsync(c => c.UserId == entityKey);
                if (coAdmin != null)
                {
                    db.CollegeCoAdmins.Remove(coAdmin);
                    await db.SaveChangesAsync();
                }

                var user = await userManager.FindByIdAsync(entityKey);
                if (user != null)
                {
                    await userManager.DeleteAsync(user);
                    _logger.LogInformation("تم حذف حساب المشرف ذو المعرف {UserId} والبريد {Email} بسبب فشل إرسال البريد بعد محاولتين.", entityKey, toEmail);
                }

                rollbackMessage = $"تعذر إرسال بيانات الدخول إلى ({toEmail}) بعد محاولتين. تم إلغاء إنشاء حساب المشرف تلقائياً من النظام، يرجى التحقق من صحة البريد وإعادة المحاولة.";
            }

            // Create notification for initiator user
            if (!string.IsNullOrEmpty(initiatorUserId))
            {
                await notificationService.SendNotificationAsync(
                    initiatorUserId,
                    "فشل إرسال البريد الإلكتروني وإلغاء العملية",
                    rollbackMessage,
                    "#",
                    "Error",
                    "bi-exclamation-triangle-fill");
            }

            var initiatorUser = !string.IsNullOrEmpty(initiatorUserId) ? await userManager.FindByIdAsync(initiatorUserId) : null;
            var initiatorRole = initiatorUser != null ? (await userManager.GetRolesAsync(initiatorUser)).FirstOrDefault() : null;

            // Record failure and rollback in AuditLog
            db.AuditLogs.Add(new AuditLog
            {
                UserId = initiatorUserId,
                UserEmail = initiatorUser?.Email ?? toEmail,
                UserName = initiatorUser?.FullName,
                UserRole = initiatorRole,
                ControllerName = "Hangfire",
                ActionName = $"Rollback_{jobType}",
                HttpMethod = "BACKGROUND",
                Path = $"/hangfire/jobs/{jobType}/{entityKey}",
                StatusCode = 500,
                IsSuccess = false,
                Description = rollbackMessage,
                ExceptionDetails = errorMessage,
                Timestamp = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }
    }
}
