using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
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

            var smtpHost = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
            var smtpPortStr = _configuration["Smtp:Port"] ?? "587";
            var smtpUser = _configuration["Smtp:User"];
            var smtpPass = _configuration["Smtp:Password"];
            var enableSsl = !bool.TryParse(_configuration["Smtp:EnableSsl"], out var ssl) || ssl;
            var senderName = _configuration["Smtp:SenderName"] ?? "منصة أخلاقيات البحث العلمي الجامعية";

            if (!string.IsNullOrEmpty(smtpHost) &&
                int.TryParse(smtpPortStr, out var smtpPort) &&
                !string.IsNullOrEmpty(smtpUser) &&
                !smtpUser.StartsWith("YOUR_") &&
                !string.IsNullOrEmpty(smtpPass) &&
                !smtpPass.StartsWith("YOUR_"))
            {
                try
                {
                    using var client = new SmtpClient(smtpHost, smtpPort)
                    {
                        EnableSsl = enableSsl,
                        Credentials = new NetworkCredential(smtpUser, smtpPass),
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        Timeout = 10000
                    };

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(smtpUser, senderName),
                        Subject = subject,
                        Body = WrapHtmlInRtlTemplate(subject, htmlMessage),
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(email);

                    await client.SendMailAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "تعذر إرسال البريد عبر Gmail SMTP، تم تسجيله في السجلات فقط.");
                }
            }
            else
            {
                // In local dev/test or when credentials pending, log clearly
                _logger.LogInformation("Gmail SMTP محاكى (في انتظار إدخال بيانات الاعتماد). البريد إلى: {Email}\nالمحتوى: {Message}", email, htmlMessage);
            }
        }

        public Task SendEmailConfirmationAsync(string email, string confirmationLink)
        {
            return SendRegistrationTokenAsync(email, "رابط التحقق المباشر", confirmationLink);
        }

        public Task SendRegistrationTokenAsync(string email, string token, string confirmationLink)
        {
            var body = $@"
                <div style='text-align: right;'>
                    <span style='display:inline-block; background-color:#e0f2fe; color:#0369a1; padding:4px 12px; border-radius:20px; font-size:12px; font-weight:600; margin-bottom:12px;'>
                        🔐 تأكيد تسجيل الحساب الأكاديمي
                    </span>
                    <h3 style='margin: 0 0 10px 0; color:#0f172a; font-size:20px;'>مرحباً بك في منصة أخلاقيات البحث العلمي</h3>
                    <p style='color:#475569; font-size:14px; line-height:1.6; margin-bottom:20px;'>
                        شكراً لانضمامكم إلى المنصة الجامعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي. لإتمام تفعيل حسابك، يرجى استخدام رمز الأمان أدناه أو النقر على زر التفعيل المباشر.
                    </p>

                    <div style='background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%); border: 2px dashed #6366f1; border-radius: 12px; padding: 20px; text-align: center; margin: 24px 0;'>
                        <div style='color: #64748b; font-size: 13px; font-weight: bold; margin-bottom: 8px;'>رمز التحقق الخاص بكم (انقر للتحديد والنسخ):</div>
                        <div style='font-family: Consolas, Monaco, monospace; font-size: 24px; font-weight: 800; letter-spacing: 3px; color: #312e81; user-select: all; -webkit-user-select: all; background:#ffffff; padding:12px; border-radius:8px; border:1px solid #cbd5e1; word-break: break-all;'>
                            {token}
                        </div>
                        <div style='color: #94a3b8; font-size: 11px; margin-top: 6px;'>* يمكنك تحديد الرمز ونسخه ولصقه في شاشة التحقق</div>
                    </div>

                    <div style='text-align: center; margin: 25px 0;'>
                        <a href='{confirmationLink}' style='display:inline-block; background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%); color: #ffffff; padding: 14px 32px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 15px; box-shadow: 0 4px 12px rgba(37,99,235,0.25);'>
                            🚀 تفعيل الحساب بنقرة واحدة
                        </a>
                    </div>

                    <p style='color:#94a3b8; font-size:12px; margin-top:20px; line-height:1.5;'>
                        إذا لم تكن أنت من قام بطلب التسجيل في المنصة، يمكنك تجاهل هذه الرسالة بأمان.
                    </p>
                </div>";

            return SendEmailAsync(email, "تأكيد الحساب ورمز التحقق - منصة أخلاقيات البحث العلمي", body);
        }

        public Task SendPasswordResetTokenAsync(string email, string token, string resetLink)
        {
            var body = $@"
                <div style='text-align: right;'>
                    <span style='display:inline-block; background-color:#fef2f2; color:#b91c1c; padding:4px 12px; border-radius:20px; font-size:12px; font-weight:600; margin-bottom:12px;'>
                        🔑 استعادة كلمة المرور
                    </span>
                    <h3 style='margin: 0 0 10px 0; color:#0f172a; font-size:20px;'>طلب إعادة تعيين كلمة المرور</h3>
                    <p style='color:#475569; font-size:14px; line-height:1.6; margin-bottom:20px;'>
                        تلقينا طلباً لإعادة تعيين كلمة المرور لحسابك في المنصة. يمكنك استخدام رمز التحقق المرفق أدناه أو النقر على زر إعادة التعيين المباشر.
                    </p>

                    <div style='background: linear-gradient(135deg, #fff7ed 0%, #ffedd5 100%); border: 2px dashed #ea580c; border-radius: 12px; padding: 20px; text-align: center; margin: 24px 0;'>
                        <div style='color: #9a3412; font-size: 13px; font-weight: bold; margin-bottom: 8px;'>رمز استعادة كلمة المرور:</div>
                        <div style='font-family: Consolas, Monaco, monospace; font-size: 24px; font-weight: 800; letter-spacing: 3px; color: #7c2d12; user-select: all; -webkit-user-select: all; background:#ffffff; padding:12px; border-radius:8px; border:1px solid #fdba74; word-break: break-all;'>
                            {token}
                        </div>
                        <div style='color: #9a3412; font-size: 11px; margin-top: 6px;'>* يمكنك تحديد الرمز ونسخه ولصقه في شاشة استعادة كلمة المرور</div>
                    </div>

                    <div style='text-align: center; margin: 25px 0;'>
                        <a href='{resetLink}' style='display:inline-block; background: linear-gradient(135deg, #dc2626 0%, #b91c1c 100%); color: #ffffff; padding: 14px 32px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 15px; box-shadow: 0 4px 12px rgba(220,38,38,0.25);'>
                            🔒 تعيين كلمة المرور الجديدة
                        </a>
                    </div>

                    <p style='color:#94a3b8; font-size:12px; margin-top:20px; line-height:1.5;'>
                        إذا لم تطلب إعادة تعيين كلمة المرور، يرجى تجاهل هذا البريد، ولن يتم إجراء أي تعديل على حسابك.
                    </p>
                </div>";

            return SendEmailAsync(email, "رمز استعادة كلمة المرور - منصة أخلاقيات البحث العلمي", body);
        }

        public Task SendJudgeInvitationAsync(string email, string invitationLink, string specialty)
        {
            var body = $@"
                <div style='text-align: right;'>
                    <span style='display:inline-block; background-color:#eff6ff; color:#1d4ed8; padding:4px 12px; border-radius:20px; font-size:12px; font-weight:600; margin-bottom:12px;'>
                        ⚖️ دعوة تحكيم أكاديمي رسمي
                    </span>
                    <h3 style='margin: 0 0 10px 0; color:#0f172a; font-size:20px;'>دعوة للانضمام إلى هيئة المحكمين</h3>
                    <p style='color:#475569; font-size:14px; line-height:1.6;'>
                        تحية طيبة وبعد،<br/>
                        يسر لجنة أخلاقيات البحث العلمي والنزاهة الأكاديمية دعوتكم رسمياً للانضمام إلى نخبة المحكمين المعتمدين في تخصص:
                        <strong style='color:#1e3a8a;'>{specialty}</strong>.
                    </p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{invitationLink}' style='display:inline-block; background: #0f172a; color: #ffffff; padding: 14px 32px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 15px;'>
                            قبول الدعوة واستكمال الملف الأكاديمي
                        </a>
                    </div>
                    <p style='color:#94a3b8; font-size:12px;'>ملاحظة: هذا الرابط الأمني صالح للاستخدام لمدة 7 أيام عمل.</p>
                </div>";

            return SendEmailAsync(email, "دعوة للانضمام إلى هيئة المحكمين - منصة أخلاقيات البحث العلمي", body);
        }

        public Task SendStatusChangedNotificationAsync(string email, string researchTitle, string oldStatus, string newStatus, string? notes = null)
        {
            var body = $@"
                <div style='text-align: right;'>
                    <h3 style='margin: 0 0 10px 0; color:#0f172a; font-size:18px;'>إشعار بتحديث حالة المقترح البحثي</h3>
                    <p style='color:#334155; font-size:15px; font-weight:bold; margin-bottom:15px;'>{researchTitle}</p>
                    <div style='background:#f8fafc; border-right:4px solid #3b82f6; padding:15px; border-radius:4px; margin-bottom:15px;'>
                        <div style='font-size:13px; color:#64748b;'>الحالة السابقة: <span style='text-decoration:line-through;'>{oldStatus}</span></div>
                        <div style='font-size:15px; color:#1e293b; font-weight:bold; margin-top:4px;'>الحالة الحالية: <span style='color:#2563eb;'>{newStatus}</span></div>
                    </div>
                    {(string.IsNullOrWhiteSpace(notes) ? "" : $"<div style='background:#fef2f2; border-right:4px solid #ef4444; padding:12px; margin-bottom:15px; font-size:13px; color:#991b1b;'><strong>ملاحظات اللجنة / المحكمين:</strong><br/>{notes}</div>")}
                    <p style='color:#64748b; font-size:13px;'>يمكنكم الدخول إلى المنصة لمتابعة الإجراءات ومراجعة المقترح.</p>
                </div>";

            return SendEmailAsync(email, $"تحديث حالة البحث: {researchTitle}", body);
        }

        public Task SendCertificateIssuedNotificationAsync(string email, string researchTitle, string certificateSerialNumber, string downloadUrl)
        {
            var body = $@"
                <div style='text-align: right;'>
                    <span style='display:inline-block; background-color:#f0fdf4; color:#15803d; padding:4px 12px; border-radius:20px; font-size:12px; font-weight:600; margin-bottom:12px;'>
                        📜 صدور الشهادة الأخلاقية الرسمية
                    </span>
                    <h3 style='margin: 0 0 10px 0; color:#0f172a; font-size:20px;'>مبارك! تم اعتماد المقترح وصدور الشهادة</h3>
                    <p style='color:#475569; font-size:14px; line-height:1.6;'>
                        يسرنا إبلاغكم باعتماد المقترح البحثي: <strong>{researchTitle}</strong> وصدور الشهادة الأخلاقية الرسمية المعتمدة برقم مسلسل:
                    </p>
                    <div style='background:#f0fdf4; border:1px solid #bbf7d0; padding:12px; border-radius:8px; text-align:center; font-size:18px; font-weight:bold; color:#166534; font-family:monospace; margin:15px 0;'>
                        {certificateSerialNumber}
                    </div>
                    <div style='text-align: center; margin: 25px 0;'>
                        <a href='{downloadUrl}' style='display:inline-block; background: #16a34a; color: #ffffff; padding: 14px 32px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 15px;'>
                            تحميل الشهادة المعتمدة (PDF)
                        </a>
                    </div>
                </div>";

            return SendEmailAsync(email, $"صدور الشهادة الأخلاقية للبحث: {researchTitle}", body);
        }

        public Task SendTemporaryPasswordAsync(string email, string tempPassword)
        {
            var body = $@"
                <div style='text-align: right;'>
                    <h3 style='margin: 0 0 10px 0; color:#0f172a; font-size:18px;'>بيانات حساب مدير اللجنة الفرعية</h3>
                    <p style='color:#475569; font-size:14px;'>تم إنشاء حساب إداري لكم بصلاحية منسق لجنة أخلاقيات البحث العلمي بالكلية.</p>
                    <div style='background:#f1f5f9; padding:15px; border-radius:8px; margin:15px 0;'>
                        <div>كلمة المرور المؤقتة: <strong style='font-family:monospace; font-size:16px; background:#fff; padding:2px 8px; border-radius:4px;'>{tempPassword}</strong></div>
                    </div>
                    <p style='color:#dc2626; font-size:13px; font-weight:bold;'>يرجى تسجيل الدخول وتغيير كلمة المرور فوراً حفاظاً على أمان النظام.</p>
                </div>";

            return SendEmailAsync(email, "بيانات الحساب الإداري - منصة أخلاقيات البحث العلمي", body);
        }

        private static string WrapHtmlInRtlTemplate(string title, string content)
        {
            return $@"
                <!DOCTYPE html>
                <html lang='ar' dir='rtl'>
                <head>
                    <meta charset='utf-8' />
                    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                    <title>{title}</title>
                </head>
                <body style='margin:0; padding:20px; background-color:#f1f5f9; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Cairo, Tahoma, Arial, sans-serif; direction: rtl; text-align: right; color: #1e293b;'>
                    <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                        <tr>
                            <td align='center'>
                                <table width='100%' style='max-width: 620px; background: #ffffff; border-radius: 14px; overflow: hidden; box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.05), 0 8px 10px -6px rgba(0, 0, 0, 0.01); border: 1px solid #e2e8f0;' cellpadding='0' cellspacing='0'>
                                    <!-- Header Banner -->
                                    <tr>
                                        <td style='background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%); padding: 28px 24px; text-align: center;'>
                                            <div style='color: #60a5fa; font-size: 13px; font-weight: 700; letter-spacing: 1px; margin-bottom: 6px; text-transform: uppercase;'>جامعة مصر الأكاديمية</div>
                                            <h1 style='margin: 0; color: #ffffff; font-size: 19px; font-weight: 700;'>منصة أخلاقيات البحث العلمي والنزاهة الأكاديمية</h1>
                                        </td>
                                    </tr>
                                    <!-- Main Content Body -->
                                    <tr>
                                        <td style='padding: 32px 28px;'>
                                            {content}
                                        </td>
                                    </tr>
                                    <!-- Footer -->
                                    <tr>
                                        <td style='background-color: #f8fafc; padding: 18px 24px; text-align: center; border-top: 1px solid #e2e8f0; font-size: 12px; color: #64748b; line-height: 1.5;'>
                                            <div>هذا بريد إلكتروني رسمي مؤتمت مرسل من منصة لجان أخلاقيات البحث العلمي.</div>
                                            <div style='margin-top: 4px;'>جميع الحقوق محفوظة &copy; {DateTime.UtcNow.Year} - جامعة مصر الأكاديمية</div>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";
        }
    }
}
