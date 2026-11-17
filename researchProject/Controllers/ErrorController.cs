using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using researchProject.Models;

namespace researchProject.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("Error/{statusCode:int}")]
        public IActionResult HandleStatusCode(int statusCode)
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            if (statusCode == 404)
            {
                var originalPath = HttpContext.Features.Get<IStatusCodeReExecuteFeature>()?.OriginalPath ?? HttpContext.Request.Path.Value;
                _logger.LogWarning("طلب صفحة أو مورد غير موجود (404): {Path}", originalPath);
                return View("NotFound", new ErrorViewModel { RequestId = requestId });
            }

            return View("GeneralError", new ErrorViewModel { RequestId = requestId });
        }

        [Route("Error/500")]
        public IActionResult HandleException()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            var ex = exceptionFeature?.Error;
            var path = exceptionFeature?.Path ?? HttpContext.Request.Path.Value;

            _logger.LogError(ex, "استثناء غير معالج على المسار {Path} - RequestId: {RequestId}", path, requestId);

            return View("GeneralError", new ErrorViewModel { RequestId = requestId });
        }
    }
}
