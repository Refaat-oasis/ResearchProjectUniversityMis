using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace researchProject.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;
        private readonly IConfiguration _configuration;

        public EmailSender(ILogger<EmailSender> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogInformation("إرسال بريد إلكتروني إلى: {Email} | الموضوع: {Subject}", email, subject);

            var smtpHost = _configuration["Smtp:Host"];
            var smtpPortStr = _configuration["Smtp:Port"];
            var smtpUser = _configuration["Smtp:User"];
            var smtpPass = _configuration["Smtp:Password"];

            if (!string.IsNullOrEmpty(smtpHost) && int.TryParse(smtpPortStr, out var smtpPort))
            {
                try
                {
                    using var client = new SmtpClient(smtpHost, smtpPort)
                    {
                        EnableSsl = true,
                        Credentials = new NetworkCredential(smtpUser, smtpPass)
                    };

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(smtpUser ?? "noreply@univ.edu.eg", "منصة النزاهة الأكاديمية وأخلاقيات البحث العلمي"),
                        Subject = subject,
                        Body = WrapHtmlInRtlTemplate(subject, htmlMessage),
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(email);

                    await client.SendMailAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "تعذر إرسال البريد عبر SMTP، تم تسجيله في السجلات فقط.");
                }
            }
            else
            {
                // In local dev/test environment, log to console/debug
                _logger.LogInformation("SMTP غير مجهز. محتوى الرسالة:\n{Message}", htmlMessage);
            }
        }

        public Task SendEmailConfirmationAsync(string email, string confirmationLink)
        {
            var body = $@"
                <p>مرحباً بك في المنصة الإلكترونية لأخلاقيات البحث العلمي والنزاهة الأكاديمية.</p>
                <p>يرجى النقر على الرابط التالي لتأكيد بريدك الإلكتروني وتفعيل حسابك الأكاديمي:</p>
                <p style='text-align:center; margin: 25px 0;'>
                    <a href='{confirmationLink}' style='background-color:#1b5e20; color:#fff; padding:12px 24px; text-decoration:none; border-radius:6px; font-weight:bold;'>تأكيد الحساب الآن</a>
                </p>
                <p>إذا لم تكن أنت من قام بهذا الطلب، يمكنك تجاهل هذا البريد بأمان.</p>";

            return SendEmailAsync(email, "تأكيد البريد الإلكتروني - منصة النزاهة الأكاديمية", body);
        }

        public Task SendJudgeInvitationAsync(string email, string invitationLink, string specialty)
        {
            var body = $@"
                <p>تحية طيبة وبعد،</p>
                <p>يسر لجنة أخلاقيات البحث العلمي دعوتكم للانضمام إلى نخبة المحكمين المعتمدين في تخصص: <strong>{specialty}</strong>.</p>
                <p>يرجى فتح الرابط التالي لاستكمال وقبول الدعوة:</p>
                <p style='text-align:center; margin: 25px 0;'>
                    <a href='{invitationLink}' style='background-color:#0d47a1; color:#fff; padding:12px 24px; text-decoration:none; border-radius:6px; font-weight:bold;'>قبول دعوة التحكيم</a>
                </p>
                <p>ملاحظة: هذا الرابط صالح لمدة 7 أيام فقط.</p>";

            return SendEmailAsync(email, "دعوة للانضمام إلى هيئة المحكمين - منصة النزاهة الأكاديمية", body);
        }

        public Task SendStatusChangedNotificationAsync(string email, string researchTitle, string oldStatus, string newStatus, string? notes = null)
        {
            var body = $@"
                <p>إشعار بتحديث حالة المقترح البحثي: <strong>{researchTitle}</strong></p>
                <p>تغيرت الحالة من: <span style='color:#757575;'>{oldStatus}</span> إلى: <strong style='color:#1565c0;'>{newStatus}</strong></p>
                {(string.IsNullOrWhiteSpace(notes) ? "" : $"<div style='background-color:#f5f5f5; padding:12px; border-right:4px solid #1976d2; margin:15px 0;'><strong>ملاحظات:</strong><br/>{notes}</div>")}
                <p>يمكنكم الدخول إلى المنصة لمتابعة الإجراءات.</p>";

            return SendEmailAsync(email, $"تحديث حالة البحث: {researchTitle}", body);
        }

        public Task SendCertificateIssuedNotificationAsync(string email, string researchTitle, string certificateSerialNumber, string downloadUrl)
        {
            var body = $@"
                <p>نهنئكم باعتماد المقترح البحثي وصدور الشهادة الأخلاقية الرسمية بنجاح!</p>
                <p>عنوان المقترح: <strong>{researchTitle}</strong></p>
                <p>الرقم المسلسل للشهادة: <strong style='color:#2e7d32; font-size:16px;'>{certificateSerialNumber}</strong></p>
                <p style='text-align:center; margin: 25px 0;'>
                    <a href='{downloadUrl}' style='background-color:#2e7d32; color:#fff; padding:12px 24px; text-decoration:none; border-radius:6px; font-weight:bold;'>تحميل الشهادة الأخلاقية (PDF)</a>
                </p>";

            return SendEmailAsync(email, $"صدور الشهادة الأخلاقية للبحث: {researchTitle}", body);
        }

        public Task SendTemporaryPasswordAsync(string email, string tempPassword)
        {
            var body = $@"
                <p>تم إنشاء حساب إداري لكم كمدير للجنة الفرعية بالكلية.</p>
                <p>كلمة المرور المؤقتة الخاصة بكم هي: <strong style='background:#eceff1; padding:4px 8px; font-family:monospace;'>{tempPassword}</strong></p>
                <p style='color:#c62828;'>يرجى تسجيل الدخول وتغيير كلمة المرور فوراً.</p>";

            return SendEmailAsync(email, "بيانات الحساب الإداري - منصة أخلاقيات البحث العلمي", body);
        }

        private static string WrapHtmlInRtlTemplate(string title, string content)
        {
            return $@"
                <!DOCTYPE html>
                <html lang='ar' dir='rtl'>
                <head>
                    <meta charset='utf-8' />
                    <title>{title}</title>
                </head>
                <body style='font-family: Cairo, Tahoma, Arial, sans-serif; direction: rtl; text-align: right; background-color: #f8f9fa; padding: 20px; color: #212529;'>
                    <div style='max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 8px; border: 1px solid #e0e0e0; overflow: hidden; box-shadow: 0 2px 4px rgba(0,0,0,0.05);'>
                        <div style='background-color: #1a365d; color: #ffffff; padding: 20px; text-align: center;'>
                            <h2 style='margin: 0; font-size: 18px;'>المنصة الإلكترونية للنزاهة الأكاديمية وأخلاقيات البحث العلمي</h2>
                        </div>
                        <div style='padding: 25px;'>
                            {content}
                        </div>
                        <div style='background-color: #f1f5f9; color: #64748b; padding: 12px; text-align: center; font-size: 12px; border-top: 1px solid #e2e8f0;'>
                            جميع الحقوق محفوظة &copy; {DateTime.UtcNow.Year} - جامعة مصر الأكاديمية
                        </div>
                    </div>
                </body>
                </html>";
        }
    }
}
