using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using researchProject.Models;
using researchProject.Models.Constants;
using researchProject.Models.ViewModels;

namespace researchProject.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole(RoleConstants.SuperAdmin))
                    return RedirectToAction("Index", "SuperAdmin");
                if (User.IsInRole(RoleConstants.CollegeCoAdmin))
                    return RedirectToAction("Index", "CollegeCoAdmin");
                if (User.IsInRole(RoleConstants.Judge))
                    return RedirectToAction("Index", "Judge");
                if (User.IsInRole(RoleConstants.Researcher))
                    return RedirectToAction("Index", "Researcher");
            }

            ViewBag.Seo = new SeoMetadataViewModel
            {
                Title = "الرئيسية | المنصة الإلكترونية لأخلاقيات البحث العلمي والنزاهة الأكاديمية",
                Description = "المنصة الإلكترونية الذكية لإدارة ومتابعة طلبات لجان أخلاقيات البحث العلمي والنزاهة الأكاديمية بالجامعة، وإصدار الشهادات الأخلاقية المعتمدة رقمياً.",
                CanonicalUrl = "/"
            };

            return View();
        }

        [HttpGet]
        public IActionResult Developers()
        {
            ViewBag.Seo = new SeoMetadataViewModel
            {
                Title = "فريق التطوير والمهندسون | منصة أخلاقيات البحث العلمي",
                Description = "تعرف على مهندسي برمجيات المنصة الإلكترونية لأخلاقيات البحث العلمي: أحمد محمد رفعت و زياد حسنين أبو الحسن.",
                CanonicalUrl = "/Home/Developers"
            };

            return View();
        }

        [HttpGet]
        public IActionResult About()
        {
            ViewBag.Seo = new SeoMetadataViewModel
            {
                Title = "عن المنصة ولجان الأخلاقيات | منصة أخلاقيات البحث العلمي",
                Description = "معلومات عن اللجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي، المعايير واللوائح الأخلاقية المعتمدة ومسار التحكيم الرقمي.",
                CanonicalUrl = "/Home/About"
            };

            return View();
        }

        [HttpGet("robots.txt")]
        public ContentResult Robots()
        {
            var sb = new StringBuilder();
            sb.AppendLine("User-agent: *");
            sb.AppendLine("Allow: /");
            sb.AppendLine("Allow: /Home/About");
            sb.AppendLine("Allow: /Home/Developers");
            sb.AppendLine("Allow: /Public/VerifyCertificate");
            sb.AppendLine("Disallow: /SuperAdmin/");
            sb.AppendLine("Disallow: /CollegeCoAdmin/");
            sb.AppendLine("Disallow: /Judge/");
            sb.AppendLine("Disallow: /Researcher/");
            sb.AppendLine("Disallow: /Account/Login");
            sb.AppendLine("Disallow: /hangfire/");
            sb.AppendLine($"Sitemap: {Request.Scheme}://{Request.Host}/sitemap.xml");

            return Content(sb.ToString(), "text/plain", Encoding.UTF8);
        }

        [HttpGet("sitemap.xml")]
        public ContentResult Sitemap()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
            sb.AppendLine($"  <url><loc>{baseUrl}/</loc><changefreq>weekly</changefreq><priority>1.0</priority></url>");
            sb.AppendLine($"  <url><loc>{baseUrl}/Home/About</loc><changefreq>monthly</changefreq><priority>0.8</priority></url>");
            sb.AppendLine($"  <url><loc>{baseUrl}/Home/Developers</loc><changefreq>monthly</changefreq><priority>0.8</priority></url>");
            sb.AppendLine($"  <url><loc>{baseUrl}/Public/VerifyCertificate</loc><changefreq>weekly</changefreq><priority>0.9</priority></url>");
            sb.AppendLine("</urlset>");

            return Content(sb.ToString(), "application/xml", Encoding.UTF8);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
