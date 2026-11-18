using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace researchProject.Services
{
    public class EmailSender : IEmailSender
    {
        private static readonly System.Threading.SemaphoreSlim _archiveLock = new(1, 1);
        private readonly ILogger<EmailSender> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _archivePath;

        public EmailSender(ILogger<EmailSender> logger, IConfiguration configuration, IWebHostEnvironment? env = null)
        {
            _logger = logger;
            _configuration = configuration;
            var basePath = env?.ContentRootPath ?? AppContext.BaseDirectory;
            _archivePath = Path.Combine(basePath, "App_Data", "SentEmails");
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogInformation("بدء معالجة إرسال بريد إلكتروني إلى: {Email} | الموضوع: {Subject}", email, subject);

            var smtpHost = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
            var smtpPortStr = _configuration["Smtp:Port"] ?? "587";
            var smtpUser = _configuration["Smtp:User"];
            var smtpPass = _configuration["Smtp:Password"];
            var enableSsl = !bool.TryParse(_configuration["Smtp:EnableSsl"], out var ssl) || ssl;
            var senderName = _configuration["Smtp:SenderName"] ?? "منصة أخلاقيات البحث العلمي الجامعية";

            var cleanUser = (smtpUser ?? "").Trim();
            var cleanPass = (smtpPass ?? "").Replace(" ", "").Trim();

            int smtpPort = 587;
            bool validPort = int.TryParse(smtpPortStr, out smtpPort);

            bool isConfigured = !string.IsNullOrWhiteSpace(smtpHost) &&
                                validPort &&
                                !string.IsNullOrWhiteSpace(cleanUser) &&
                                !cleanUser.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase) &&
                                !string.IsNullOrWhiteSpace(cleanPass) &&
                                !cleanPass.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase);

            var fullHtml = WrapHtmlInRtlTemplate(subject, htmlMessage);
            string deliveryStatus = "Simulated";
            string? errorDetails = null;

            if (isConfigured)
            {
                try
                {
                    using var client = new SmtpClient(smtpHost, smtpPort)
                    {
                        EnableSsl = enableSsl,
                        Credentials = new NetworkCredential(cleanUser, cleanPass),
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        Timeout = 15000
                    };

                    using var mailMessage = new MailMessage
                    {
                        From = new MailAddress(cleanUser, senderName, Encoding.UTF8),
                        Subject = subject,
                        SubjectEncoding = Encoding.UTF8,
                        Body = fullHtml,
                        BodyEncoding = Encoding.UTF8,
                        HeadersEncoding = Encoding.UTF8,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(email);

                    await client.SendMailAsync(mailMessage);
                    deliveryStatus = "DeliveredViaSmtp";
                    _logger.LogInformation("✅ تم إرسال البريد الإلكتروني بنجاح عبر SMTP ({Host}:{Port}) إلى: {Email}", smtpHost, smtpPort, email);
                }
                catch (Exception ex)
                {
                    deliveryStatus = "Failed";
                    errorDetails = ex.Message;
                    _logger.LogError(ex, "❌ تعذر إرسال البريد عبر SMTP ({Host}:{Port}) إلى {Email}. السبب: {Error}\nتنبيه: في حال استخدام Gmail، يرجى التأكد من تفعيل التحقق بخطوتين (2FA) واستخدام كلمة مرور التطبيقات (App Password) المكونة من 16 حرفاً من إعدادات Google Account وليس كلمة مرور الحساب العادية.", smtpHost, smtpPort, email, ex.Message);
                }
            }
            else
            {
                deliveryStatus = "SimulatedLocal";
                _logger.LogWarning("⚠️ إعدادات خادم البريد (Smtp:User / Smtp:Password) غير مجهزة أو تستخدم قيماً افتراضية (YOUR_...). تم تشغيل وضع المحاكاة وحفظ الرسالة محلياً. البريد إلى: {Email}", email);
            }

            // Always archive a copy locally in App_Data/SentEmails so developers/users never lose verification codes or links
            await SaveEmailToLocalArchiveAsync(email, subject, fullHtml, deliveryStatus, errorDetails);
        }

        private async Task SaveEmailToLocalArchiveAsync(string email, string subject, string fullHtml, string deliveryStatus, string? errorDetails)
        {
            try
            {
                if (!Directory.Exists(_archivePath))
                {
                    Directory.CreateDirectory(_archivePath);
                }

                var now = DateTime.UtcNow;
                string fileName;
                var s = subject ?? "";
                if (s.Contains("دعوة", StringComparison.OrdinalIgnoreCase))
                {
                    fileName = "last_invitation.html";
                }
                else if (s.Contains("رمز", StringComparison.OrdinalIgnoreCase) || s.Contains("تفعيل", StringComparison.OrdinalIgnoreCase) || s.Contains("OTP", StringComparison.OrdinalIgnoreCase))
                {
                    fileName = "last_otp.html";
                }
                else if (s.Contains("بيانات الحساب", StringComparison.OrdinalIgnoreCase) || s.Contains("كلمة المرور المؤقتة", StringComparison.OrdinalIgnoreCase))
                {
                    fileName = "last_new_account.html";
                }
                else
                {
                    fileName = "last_notification.html";
                }

                var filePath = Path.Combine(_archivePath, fileName);

                var statusBadgeColor = deliveryStatus switch
                {
                    "DeliveredViaSmtp" => "#16a34a",
                    "SimulatedLocal" => "#d97706",
                    _ => "#dc2626"
                };

                var statusBadgeText = deliveryStatus switch
                {
                    "DeliveredViaSmtp" => "✅ تم الإرسال الفعلي عبر خادم SMTP",
                    "SimulatedLocal" => "⚠️ محاكاة محلية (في انتظار إدخال بيانات اعتماد SMTP)",
                    _ => $"❌ فشل الإرسال عبر SMTP ({errorDetails})"
                };

                var safeEmail = WebUtility.HtmlEncode(email ?? "");
                var safeSubject = WebUtility.HtmlEncode(subject ?? "");

                var diagnosticBanner = $@"
<div style='background-color: #0f172a; color: #f8fafc; padding: 12px 18px; font-family: Cairo, Tahoma, sans-serif; font-size: 13px; border-bottom: 3px solid {statusBadgeColor}; direction: rtl; text-align: right;'>
    <div style='display:flex; justify-content:space-between; align-items:center;'>
        <span><strong>📨 سجل البريد الصادر (Local Mail Archive: {fileName})</strong></span>
        <span style='background-color: {statusBadgeColor}; color:#ffffff; padding:3px 10px; border-radius:12px; font-weight:bold; font-size:12px;'>{statusBadgeText}</span>
    </div>
    <div style='margin-top: 6px; color: #94a3b8;'>
        <strong>المستلم:</strong> {safeEmail} &nbsp;|&nbsp; 
        <strong>الموضوع:</strong> {safeSubject} &nbsp;|&nbsp; 
        <strong>التوقيت (UTC):</strong> {now:yyyy-MM-dd HH:mm:ss}
    </div>
</div>
";

                var combinedHtml = diagnosticBanner + fullHtml;
                await _archiveLock.WaitAsync();
                try
                {
                    await File.WriteAllTextAsync(filePath, combinedHtml, Encoding.UTF8);
                }
                finally
                {
                    _archiveLock.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "تعذر حفظ نسخة البريد الصادر في الأرشيف المحلي.");
            }
        }

        public Task SendEmailConfirmationAsync(string email, string confirmationLink)
        {
            return SendRegistrationTokenAsync(email, "رابط التحقق المباشر", confirmationLink);
        }

        public Task SendRegistrationTokenAsync(string email, string token, string confirmationLink)
        {
            var body = $@"
                <div style='text-align: right; font-family: Cairo, Tahoma, Arial, sans-serif;'>
                    <div style='display:inline-block; background-color:#eff6ff; color:#1d4ed8; padding:6px 14px; border-radius:20px; font-size:12px; font-weight:700; margin-bottom:12px; border:1px solid #bfdbfe;'>
                        🔐 تأكيد تسجيل الحساب الأكاديمي
                    </div>
                    <h3 style='margin: 0 0 10px 0; color:#0f172a; font-size:22px; font-weight:700;'>مرحباً بك في منصة أخلاقيات البحث العلمي</h3>
                    <p style='color:#475569; font-size:14px; line-height:1.7; margin-bottom:20px;'>
                        شكراً لانضمامكم إلى المنصة الجامعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي. لإتمام تفعيل حسابك، يرجى استخدام رمز الأمان المكوّن من 6 أرقام أدناه أو النقر على زر التفعيل المباشر.
                    </p>

                    <!-- Alert for 15-minute expiration -->
                    <div style='background-color: #fffbeb; border: 1px solid #fde68a; border-radius: 8px; padding: 12px 16px; margin-bottom: 20px;'>
                        <span style='color: #b45309; font-size: 13px; font-weight: 600;'>
                            ⏰ تنبيه: رمز التحقق صالح للاستخدام لمدة <strong>15 دقيقة فقط</strong> من وقت إرسال هذه الرسالة.
                        </span>
                    </div>

                    <!-- 6-digit OTP Box (Email Client Safe) -->
                    <div style='background: linear-gradient(135deg, #f8fafc 0%, #eef2f6 100%); border: 2px dashed #3b82f6; border-radius: 14px; padding: 24px; text-align: center; margin: 24px 0;'>
                        <div style='color: #475569; font-size: 13px; font-weight: bold; margin-bottom: 10px;'>رمز التحقق الرقمي الخاص بك (OTP):</div>
                        <div onclick=""if(typeof copyOtpToClipboard==='function'){{copyOtpToClipboard(this, '{token}');}}"" title='انقر لنسخ الرمز' style='font-family: ""Courier New"", Monaco, monospace; font-size: 34px; font-weight: 800; letter-spacing: 8px; color: #1e3a8a; user-select: all; -webkit-user-select: all; -moz-user-select: all; background:#ffffff; padding:12px 24px; border-radius:10px; border:2px solid #93c5fd; display:inline-block; margin: 0 auto 10px auto; cursor: pointer;'>
                            {token}
                        </div>
                        <div style='margin-top: 10px;'>
                            <a href='{confirmationLink}' target='_blank' onclick=""if(typeof copyOtpToClipboard==='function'){{copyOtpToClipboard(this, '{token}'); return false;}} else {{navigator.clipboard.writeText('{token}'); alert('تم نسخ رمز التحقق بنجاح: {token}'); return false;}}"" style='display:inline-block; text-decoration:none; cursor:pointer; background:#2563eb; color:#ffffff; padding:10px 24px; border-radius:8px; font-weight:700; font-size:14px; box-shadow:0 2px 6px rgba(37,99,235,0.3); font-family:Cairo, Tahoma, Arial, sans-serif;'>
                                📋 نسخ رمز التحقق
                            </a>
                        </div>
                        <div style='color: #64748b; font-size: 12px; margin-top: 12px; line-height: 1.6;'>
                            💡 <strong>طرق استخدام الرمز:</strong><br/>
                            1. انقر على زر <strong>نسخ رمز التحقق</strong> أعلاه (في المتصفح ينسخ الرمز فوراً، وفي بريد Gmail ينقلك مباشرة لتأكيد حسابك وتعبئة الرمز تلقائياً).<br/>
                            2. أو انقر نقراً مزدوجاً على أرقام الرمز أعلاه لتحديده ونسخه يدوياً (Ctrl+C).<br/>
                            3. أو انقر على زر التفعيل المباشر أدناه لإتمام التفعيل بنقرة واحدة.
                        </div>
                    </div>

                    <!-- 1-Click Verification Link with Auto-Fill & Auto-Copy -->
                    <div style='text-align: center; margin: 28px 0;'>
                        <a href='{confirmationLink}' style='display:inline-block; background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%); color: #ffffff; padding: 14px 34px; text-decoration: none; border-radius: 8px; font-weight: 700; font-size: 15px; box-shadow: 0 4px 14px rgba(37,99,235,0.25);'>
                            🚀 تفعيل الحساب وتعبئة الرمز تلقائياً بنقرة واحدة
                        </a>
                    </div>

                    <p style='color:#94a3b8; font-size:12px; margin-top:24px; line-height:1.6;'>
                        إذا لم تكن أنت من قام بطلب التسجيل في المنصة، يمكنك تجاهل هذه الرسالة بأمان دون أي إجراء.
                    </p>
                </div>";

            return SendEmailAsync(email, "رمز التحقق وتفعيل الحساب (صالح لـ 15 دقيقة) - منصة أخلاقيات البحث العلمي", body);
        }

        public Task SendPasswordResetTokenAsync(string email, string token, string resetLink)
        {
            var body = $@"
                <div style='text-align: right; font-family: Cairo, Tahoma, Arial, sans-serif;'>
                    <div style='display:inline-block; background-color:#fef2f2; color:#b91c1c; padding:6px 14px; border-radius:20px; font-size:12px; font-weight:700; margin-bottom:12px; border:1px solid #fecaca;'>
                        🔑 استعادة كلمة المرور
                    </div>
                    <h3 style='margin: 0 0 10px 0; color:#0f172a; font-size:22px; font-weight:700;'>طلب إعادة تعيين كلمة المرور</h3>
                    <p style='color:#475569; font-size:14px; line-height:1.7; margin-bottom:20px;'>
                        تلقينا طلباً لإعادة تعيين كلمة المرور لحسابك في المنصة. يرجى استخدام رمز الأمان المكوّن من 6 أرقام أدناه أو النقر على زر إعادة التعيين المباشر.
                    </p>

                    <!-- Alert for 15-minute expiration -->
                    <div style='background-color: #fff7ed; border: 1px solid #ffedd5; border-radius: 8px; padding: 12px 16px; margin-bottom: 20px;'>
                        <span style='color: #c2410c; font-size: 13px; font-weight: 600;'>
                            ⏰ تنبيه: رمز استعادة كلمة المرور صالح للاستخدام لمدة <strong>15 دقيقة فقط</strong> من وقت إرسال هذه الرسالة.
                        </span>
                    </div>

                    <!-- 6-digit OTP Box (Email Client Safe) -->
                    <div style='background: linear-gradient(135deg, #fff7ed 0%, #ffedd5 100%); border: 2px dashed #ea580c; border-radius: 14px; padding: 24px; text-align: center; margin: 24px 0;'>
                        <div style='color: #9a3412; font-size: 13px; font-weight: bold; margin-bottom: 10px;'>رمز استعادة كلمة المرور (OTP):</div>
                        <div onclick=""if(typeof copyOtpToClipboard==='function'){{copyOtpToClipboard(this, '{token}');}}"" title='انقر لنسخ الرمز' style='font-family: ""Courier New"", Monaco, monospace; font-size: 34px; font-weight: 800; letter-spacing: 8px; color: #7c2d12; user-select: all; -webkit-user-select: all; -moz-user-select: all; background:#ffffff; padding:12px 24px; border-radius:10px; border:2px solid #fdba74; display:inline-block; margin: 0 auto 10px auto; cursor: pointer;'>
                            {token}
                        </div>
                        <div style='margin-top: 10px;'>
                            <a href='{resetLink}' target='_blank' onclick=""if(typeof copyOtpToClipboard==='function'){{copyOtpToClipboard(this, '{token}'); return false;}} else {{navigator.clipboard.writeText('{token}'); alert('تم نسخ رمز الاستعادة بنجاح: {token}'); return false;}}"" style='display:inline-block; text-decoration:none; cursor:pointer; background:#ea580c; color:#ffffff; padding:10px 24px; border-radius:8px; font-weight:700; font-size:14px; box-shadow:0 2px 6px rgba(234,88,12,0.3); font-family:Cairo, Tahoma, Arial, sans-serif;'>
                                📋 نسخ رمز الاستعادة
                            </a>
                        </div>
                        <div style='color: #9a3412; font-size: 12px; margin-top: 12px; line-height: 1.6;'>
                            💡 <strong>طرق استخدام الرمز:</strong><br/>
                            1. انقر على زر <strong>نسخ رمز الاستعادة</strong> أعلاه لنسخه تلقائياً أو فتح صفحة التعيين مباشرة.<br/>
                            2. أو انقر نقراً مزدوجاً على أرقام الرمز أعلاه لتحديده ونسخه يدوياً (Ctrl+C).<br/>
                            3. أو استخدم زر تعيين كلمة المرور أدناه لفتح النموذج وتعبئة الرمز مباشرة.
                        </div>
                    </div>

                    <!-- 1-Click Direct Reset Link with Auto-Fill -->
                    <div style='text-align: center; margin: 28px 0;'>
                        <a href='{resetLink}' style='display:inline-block; background: linear-gradient(135deg, #dc2626 0%, #b91c1c 100%); color: #ffffff; padding: 14px 34px; text-decoration: none; border-radius: 8px; font-weight: 700; font-size: 15px; box-shadow: 0 4px 14px rgba(220,38,38,0.25);'>
                            🔒 تعيين كلمة المرور الجديدة وتعبئة الرمز تلقائياً
                        </a>
                    </div>

                    <p style='color:#94a3b8; font-size:12px; margin-top:24px; line-height:1.6;'>
                        إذا لم تطلب إعادة تعيين كلمة المرور، يرجى تجاهل هذا البريد، ولن يتم إجراء أي تعديل على حسابك.
                    </p>
                </div>";

            return SendEmailAsync(email, "رمز استعادة كلمة المرور (صالح لـ 15 دقيقة) - منصة أخلاقيات البحث العلمي", body);
        }

        public Task SendJudgeInvitationAsync(string email, string invitationLink, string specialty, string? collegeName = null)
        {
            var collegeTitle = !string.IsNullOrWhiteSpace(collegeName) ? $"بـ {WebUtility.HtmlEncode(collegeName)}" : "";
            var safeSpecialty = WebUtility.HtmlEncode(specialty ?? "");
            var body = $@"
                <div style='text-align: right;'>
                    <span style='display:inline-block; background-color:#eff6ff; color:#1d4ed8; padding:4px 12px; border-radius:20px; font-size:12px; font-weight:600; margin-bottom:12px;'>
                        ⚖️ دعوة تحكيم أكاديمي رسمي {collegeTitle}
                    </span>
                    <h3 style='margin: 0 0 10px 0; color:#0f172a; font-size:20px;'>دعوة للانضمام إلى هيئة المحكمين {collegeTitle}</h3>
                    <p style='color:#475569; font-size:14px; line-height:1.6;'>
                        تحية طيبة وبعد،<br/>
                        يسر لجنة أخلاقيات البحث العلمي والنزاهة الأكاديمية {collegeTitle} دعوتكم رسمياً للانضمام إلى نخبة المحكمين المعتمدين في تخصص:
                        <strong style='color:#1e3a8a;'>{safeSpecialty}</strong>.
                    </p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{invitationLink}' style='display:inline-block; background: #0f172a; color: #ffffff; padding: 14px 32px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 15px;'>
                            قبول الدعوة واستكمال الملف الأكاديمي
                        </a>
                    </div>
                    <p style='color:#94a3b8; font-size:12px;'>ملاحظة: هذا الرابط الأمني صالح للاستخدام لمدة 7 أيام عمل.</p>
                </div>";

            return SendEmailAsync(email, $"دعوة للانضمام إلى هيئة المحكمين {collegeTitle} - منصة أخلاقيات البحث العلمي", body);
        }

        public Task SendStatusChangedNotificationAsync(string email, string researchTitle, string oldStatus, string newStatus, string? notes = null)
        {
            var safeTitle = WebUtility.HtmlEncode(researchTitle ?? "");
            var safeOldStatus = WebUtility.HtmlEncode(oldStatus ?? "");
            var safeNewStatus = WebUtility.HtmlEncode(newStatus ?? "");
            var safeNotes = !string.IsNullOrWhiteSpace(notes) ? WebUtility.HtmlEncode(notes) : null;

            var body = $@"
                <div style='text-align: right;'>
                    <h3 style='margin: 0 0 10px 0; color:#0f172a; font-size:18px;'>إشعار بتحديث حالة المقترح البحثي</h3>
                    <p style='color:#334155; font-size:15px; font-weight:bold; margin-bottom:15px;'>{safeTitle}</p>
                    <div style='background:#f8fafc; border-right:4px solid #3b82f6; padding:15px; border-radius:4px; margin-bottom:15px;'>
                        <div style='font-size:13px; color:#64748b;'>الحالة السابقة: <span style='text-decoration:line-through;'>{safeOldStatus}</span></div>
                        <div style='font-size:15px; color:#1e293b; font-weight:bold; margin-top:4px;'>الحالة الحالية: <span style='color:#2563eb;'>{safeNewStatus}</span></div>
                    </div>
                    {(string.IsNullOrWhiteSpace(safeNotes) ? "" : $"<div style='background:#fef2f2; border-right:4px solid #ef4444; padding:12px; margin-bottom:15px; font-size:13px; color:#991b1b;'><strong>ملاحظات اللجنة / المحكمين:</strong><br/>{safeNotes}</div>")}
                    <p style='color:#64748b; font-size:13px;'>يمكنكم الدخول إلى المنصة لمتابعة الإجراءات ومراجعة المقترح.</p>
                </div>";

            return SendEmailAsync(email, $"تحديث حالة البحث: {researchTitle}", body);
        }

        public Task SendCertificateIssuedNotificationAsync(string email, string researchTitle, string certificateSerialNumber, string downloadUrl)
        {
            var safeTitle = WebUtility.HtmlEncode(researchTitle ?? "");
            var safeSerial = WebUtility.HtmlEncode(certificateSerialNumber ?? "");
            var body = $@"
                <div style='text-align: right;'>
                    <span style='display:inline-block; background-color:#f0fdf4; color:#15803d; padding:4px 12px; border-radius:20px; font-size:12px; font-weight:600; margin-bottom:12px;'>
                        📜 صدور الشهادة الأخلاقية الرسمية
                    </span>
                    <h3 style='margin: 0 0 10px 0; color:#0f172a; font-size:20px;'>مبارك! تم اعتماد المقترح وصدور الشهادة</h3>
                    <p style='color:#475569; font-size:14px; line-height:1.6;'>
                        يسرنا إبلاغكم باعتماد المقترح البحثي: <strong>{safeTitle}</strong> وصدور الشهادة الأخلاقية الرسمية المعتمدة برقم مسلسل:
                    </p>
                    <div style='background:#f0fdf4; border:1px solid #bbf7d0; padding:12px; border-radius:8px; text-align:center; font-size:18px; font-weight:bold; color:#166534; font-family:monospace; margin:15px 0;'>
                        {safeSerial}
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
                <div style='text-align: right; font-family: Cairo, Tahoma, Arial, sans-serif;'>
                    <div style='display:inline-block; background-color:#eff6ff; color:#1d4ed8; padding:6px 14px; border-radius:20px; font-size:12px; font-weight:700; margin-bottom:12px; border:1px solid #bfdbfe;'>
                        🛡️ بيانات الحساب الإداري الجديد
                    </div>
                    <h3 style='margin: 0 0 10px 0; color:#0f172a; font-size:20px; font-weight:700;'>بيانات حساب مدير اللجنة الفرعية</h3>
                    <p style='color:#475569; font-size:14px; line-height:1.6;'>
                        تم إنشاء حساب إداري لكم بصلاحية منسق لجنة أخلاقيات البحث العلمي بالكلية. يرجى استخدام كلمة المرور المؤقتة أدناه لتسجيل الدخول:
                    </p>
                    <div style='background: #f8fafc; border: 2px dashed #0284c7; border-radius: 12px; padding: 20px; text-align: center; margin: 20px 0;'>
                        <div style='color: #475569; font-size: 13px; font-weight: bold; margin-bottom: 8px;'>كلمة المرور المؤقتة:</div>
                        <div style='font-family: ""Courier New"", Monaco, monospace; font-size: 26px; font-weight: 800; color: #0369a1; background: #ffffff; padding: 10px 24px; border-radius: 8px; border: 1px solid #bae6fd; display: inline-block; user-select: all; -webkit-user-select: all; -moz-user-select: all;'>
                            {tempPassword}
                        </div>
                        <div style='color: #64748b; font-size: 12px; margin-top: 6px;'>
                            * انقر نقراً مزدوجاً على كلمة المرور أعلاه لتحديدها ونسخها مباشرة
                        </div>
                    </div>
                    <div style='background-color: #fef2f2; border: 1px solid #fecaca; border-radius: 8px; padding: 12px 16px; margin-bottom: 20px;'>
                        <span style='color: #dc2626; font-size: 13px; font-weight: 600;'>
                            ⚠️ تنبيه أمني: يرجى تسجيل الدخول وتغيير كلمة المرور فوراً من صفحة الملف الشخصي حفاظاً على سرية البيانات.
                        </span>
                    </div>
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
                    <script>
                        function copyOtpToClipboard(btn, text) {{
                            try {{
                                if (navigator.clipboard && window.isSecureContext) {{
                                    navigator.clipboard.writeText(text).then(function() {{
                                        notifyCopied(btn, text);
                                    }}).catch(function() {{
                                        execCommandFallback(btn, text);
                                    }});
                                    return;
                                }}
                            }} catch(e) {{}}
                            execCommandFallback(btn, text);
                        }}

                        function execCommandFallback(btn, text) {{
                            try {{
                                var textArea = document.createElement('textarea');
                                textArea.value = text;
                                textArea.style.position = 'fixed';
                                textArea.style.top = '-9999px';
                                textArea.style.left = '-9999px';
                                textArea.setAttribute('readonly', '');
                                document.body.appendChild(textArea);
                                textArea.focus();
                                textArea.select();
                                var successful = false;
                                try {{
                                    successful = document.execCommand('copy');
                                }} catch (err) {{
                                    successful = false;
                                }}
                                document.body.removeChild(textArea);
                                if (successful) {{
                                    notifyCopied(btn, text);
                                    return;
                                }}
                            }} catch (e) {{}}
                            prompt('يرجى نسخ رمز التحقق يدوياً بالضغط على Ctrl+C ثم Enter:', text);
                        }}

                        function notifyCopied(btn, text) {{
                            if (btn) {{
                                var origText = btn.getAttribute('data-original-text') || btn.innerText;
                                btn.setAttribute('data-original-text', origText);
                                btn.innerText = '✅ تم نسخ الرمز: ' + text;
                                btn.style.backgroundColor = '#16a34a';
                                setTimeout(function() {{
                                    btn.innerText = origText;
                                    btn.style.backgroundColor = '';
                                }}, 3500);
                            }}
                            alert('تم نسخ رمز التحقق بنجاح إلى الحافظة: ' + text);
                        }}
                    </script>
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
