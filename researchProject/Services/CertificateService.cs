using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using QRCoder;
using researchProject.Models.Entities;

namespace researchProject.Services
{
    public class CertificateService : ICertificateService
    {
        private readonly IWebHostEnvironment _env;

        public CertificateService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public string GenerateSerialNumber(string collegeCode)
        {
            var year = DateTime.UtcNow.Year;
            var randomNum = RandomNumberGenerator.GetInt32(100000, 1000000);
            return $"REC-{collegeCode.ToUpperInvariant()}-{year}-{randomNum}";
        }

        public string GenerateQrCodeBase64(string text)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(15);
            return Convert.ToBase64String(qrCodeBytes);
        }

        public async Task<string> GenerateCertificateFileAsync(Research research, CollegeCoAdmin issuingCoAdmin, string baseUrl)
        {
            var serialNumber = research.CertificateSerialNumber ?? GenerateSerialNumber(research.College?.Code ?? "MIS");
            var qrUrl = $"{baseUrl.TrimEnd('/')}/Public/VerifyCertificate?serialNumber={Uri.EscapeDataString(serialNumber)}";
            var qrBase64 = GenerateQrCodeBase64(qrUrl);

            var certDir = Path.Combine(_env.ContentRootPath, "App_Data", "Certificates");
            if (!Directory.Exists(certDir))
            {
                Directory.CreateDirectory(certDir);
            }

            var fileName = $"Cert_{serialNumber}_{Guid.NewGuid():N}.html";
            var filePath = Path.Combine(certDir, fileName);

            // Read issuing co-admin signature if exists
            string signatureHtml = "<div style='font-style:italic; color:#757575;'>[ختم وتوقيع إلكتروني موثق من اللجنة]</div>";
            if (!string.IsNullOrEmpty(issuingCoAdmin.SignatureImagePath))
            {
                var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath ?? Directory.GetCurrentDirectory(), "wwwroot");
                var cleanRelative = issuingCoAdmin.SignatureImagePath.TrimStart('~', '/', '\\');
                var sigFullPath = Path.GetFullPath(Path.Combine(webRoot, cleanRelative));
                if (sigFullPath.StartsWith(webRoot, StringComparison.OrdinalIgnoreCase) && File.Exists(sigFullPath))
                {
                    var sigBytes = await File.ReadAllBytesAsync(sigFullPath);
                    var sigBase64 = Convert.ToBase64String(sigBytes);
                    signatureHtml = $"<img src='data:image/png;base64,{sigBase64}' style='max-height:80px; max-width:200px;' alt='توقيع مدير اللجنة' />";
                }
            }

            var safeCollegeName = System.Net.WebUtility.HtmlEncode(research.College?.Name ?? string.Empty);
            var safeTitle = System.Net.WebUtility.HtmlEncode(research.Title ?? string.Empty);
            var safeSpecialty = System.Net.WebUtility.HtmlEncode(research.AcademicSpecialty ?? string.Empty);
            var safeCategoryName = System.Net.WebUtility.HtmlEncode(research.Category?.Name ?? string.Empty);
            var safeCoAdminName = System.Net.WebUtility.HtmlEncode(issuingCoAdmin.User?.FullName ?? string.Empty);

            var researchersList = new StringBuilder();
            if (research.ResearcherResearchesBridges != null)
            {
                foreach (var bridge in research.ResearcherResearchesBridges)
                {
                    var roleText = bridge.IsMainAuthor ? "(الباحث الرئيسي)" : "(باحث مشارك)";
                    var safeAuthorName = System.Net.WebUtility.HtmlEncode(bridge.Researcher?.User?.FullName ?? string.Empty);
                    researchersList.Append($"<span style='margin-left: 15px;'><strong>{safeAuthorName}</strong> {roleText}</span>");
                }
            }

            var htmlContent = $@"<!DOCTYPE html>
<html lang='ar' dir='rtl'>
<head>
    <meta charset='utf-8' />
    <title>شهادة اعتماد أخلاقيات البحث العلمي - {serialNumber}</title>
    <style>
        @page {{ size: A4 landscape; margin: 15mm; }}
        body {{
            font-family: 'Cairo', 'Segoe UI', Tahoma, Arial, sans-serif;
            direction: rtl;
            text-align: right;
            background-color: #fdfdfd;
            margin: 0;
            padding: 20px;
            color: #1a202c;
        }}
        .certificate-border {{
            border: 6px double #1a365d;
            padding: 35px;
            background: #ffffff;
            position: relative;
            box-shadow: 0 4px 12px rgba(0,0,0,0.08);
            border-radius: 8px;
        }}
        .header {{
            display: flex;
            justify-content: space-between;
            align-items: center;
            border-bottom: 2px solid #cbd5e1;
            padding-bottom: 20px;
            margin-bottom: 25px;
        }}
        .univ-title {{
            font-size: 22px;
            font-weight: bold;
            color: #1e3a8a;
            margin: 0;
        }}
        .committee-title {{
            font-size: 16px;
            color: #475569;
            margin-top: 5px;
        }}
        .cert-badge {{
            text-align: center;
            margin: 20px 0;
        }}
        .cert-badge h1 {{
            font-size: 28px;
            color: #0f172a;
            letter-spacing: 1px;
            margin: 0;
            text-decoration: underline;
        }}
        .cert-meta {{
            font-size: 15px;
            color: #334155;
            margin-top: 10px;
        }}
        .content-box {{
            line-height: 2.1;
            font-size: 17px;
            margin: 30px 0;
        }}
        .highlight {{
            font-weight: bold;
            color: #1e40af;
        }}
        .footer {{
            display: flex;
            justify-content: space-between;
            align-items: flex-end;
            margin-top: 40px;
            border-top: 1px dashed #cbd5e1;
            padding-top: 20px;
        }}
        .qr-section {{
            text-align: center;
        }}
        .qr-section img {{
            width: 120px;
            height: 120px;
            border: 1px solid #cbd5e1;
            padding: 4px;
            background: #fff;
        }}
        .qr-label {{
            font-size: 12px;
            color: #64748b;
            margin-top: 4px;
        }}
        .signature-section {{
            text-align: center;
            min-width: 250px;
        }}
        .signature-section p {{
            margin: 5px 0;
            font-size: 15px;
        }}
        @media print {{
            body {{ padding: 0; background: none; }}
            .certificate-border {{ box-shadow: none; }}
        }}
    </style>
</head>
<body>
    <div class='certificate-border'>
        <div class='header'>
            <div>
                <div class='univ-title'>جامعة مصر الأكاديمية</div>
                <div class='committee-title'>{research.College?.Name} - اللجنة الفرعية لأخلاقيات البحث العلمي والنزاهة الأكاديمية</div>
            </div>
            <div style='text-align: left;'>
                <div style='font-size: 13px; color: #64748b;'>الرقم المسلسل:</div>
                <div style='font-size: 16px; font-weight: bold; color: #047857; direction: ltr;'>{serialNumber}</div>
            </div>
        </div>

        <div class='cert-badge'>
            <h1>شهادة اعتماد وموافقة أخلاقية</h1>
            <div class='cert-meta'>Ethical Clearance Certificate</div>
        </div>

        <div class='content-box'>
            <p>
                تشهد اللجنة الفرعية لأخلاقيات البحث العلمي والنزاهة الأكاديمية بـ <span class='highlight'>{research.College?.Name}</span>
                بأن المقترح البحثي المقدم بعنوان:
            </p>
            <div style='background-color: #f1f5f9; padding: 15px 20px; border-right: 5px solid #1e40af; border-radius: 4px; font-size: 18px; font-weight: bold; color: #0f172a;'>
                « {research.Title} »
            </div>
            <p style='margin-top: 18px;'>
                المقدم من الباحثين: <br/>
                {researchersList}
            </p>
            <p>
                التخصص الأكاديمي: <span class='highlight'>{research.AcademicSpecialty}</span> | تصنيف البحث: <span class='highlight'>{research.Category?.Name}</span>
            </p>
            <p style='text-align: justify;'>
                قد خضع للمراجعة والتحكيم الأخلاقي الشامل من قِبل نخبة من المحكمين المعتمدين باللجنة، وثبت استيفاؤه لكافة معايير
                النزاهة الأكاديمية وحماية الخصوصية ومبادئ الموافقة المستنيرة وتدابير إدارة المخاطر المعتمدة.
                <strong>وبناءً عليه تم منح هذا المقترح البحثي الموافقة والاعتماد الأخلاقي النهائي.</strong>
            </p>
        </div>

        <div class='footer'>
            <div class='qr-section'>
                <img src='data:image/png;base64,{qrBase64}' alt='رمز التحقق الرقمي' />
                <div class='qr-label'>امسح الرمز للتحقق من صحة الوثيقة رسمياً</div>
            </div>

            <div style='text-align: center;'>
                <div style='font-size: 14px; color: #475569;'>تاريخ الإصدار والاعتماد</div>
                <div style='font-size: 16px; font-weight: bold; color: #1e293b; margin-top: 4px;'>{DateTime.UtcNow:yyyy/MM/dd}</div>
                <div style='font-size: 12px; color: #94a3b8;'>صلاحية الشهادة: سنتان من تاريخه</div>
            </div>

            <div class='signature-section'>
                <p style='font-weight: bold; color: #1e3a8a;'>رئيس / مدير اللجنة الفرعية بالكلية</p>
                <div style='margin: 10px 0;'>
                    {signatureHtml}
                </div>
                <p style='font-size: 14px; font-weight: bold;'>{issuingCoAdmin.User?.FullName}</p>
            </div>
        </div>
    </div>
</body>
</html>";

            await File.WriteAllTextAsync(filePath, htmlContent, Encoding.UTF8);
            return fileName;
        }

        public async Task<byte[]?> GetCertificateFileBytesAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return null;

            var contentRoot = _env.ContentRootPath ?? Directory.GetCurrentDirectory();
            var certDir = Path.Combine(contentRoot, "App_Data", "Certificates");
            var fullPath = Path.Combine(certDir, fileName);

            if (!File.Exists(fullPath)) return null;

            return await File.ReadAllBytesAsync(fullPath);
        }
    }
}
