using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using researchProject.Data;
using researchProject.Models.Constants;
using researchProject.Models.Entities;

namespace researchProject.Filters
{
    public class AuditLogFilter : IAsyncActionFilter
    {
        private readonly ILogger<AuditLogFilter> _logger;

        public AuditLogFilter(ILogger<AuditLogFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Skip logging for static assets or noise endpoints if any routed through MVC
            var path = context.HttpContext.Request.Path.Value ?? string.Empty;
            if (path.StartsWith("/hubs", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/hangfire", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/Error", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".css", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".ico", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            ActionExecutedContext? executedContext = null;
            Exception? caughtException = null;

            try
            {
                executedContext = await next();
                if (executedContext.Exception != null && !executedContext.ExceptionHandled)
                {
                    caughtException = executedContext.Exception;
                }
            }
            catch (Exception ex)
            {
                caughtException = ex;
                throw;
            }
            finally
            {
                stopwatch.Stop();

                try
                {
                    // Use a fresh DbContext scope to ensure log persistence even if transaction failed
                    var serviceScopeFactory = context.HttpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();
                    using var scope = serviceScopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var httpContext = context.HttpContext;
                    var user = httpContext.User;

                    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                    var userEmail = user.FindFirstValue(ClaimTypes.Email) ?? user.Identity?.Name;
                    var userName = user.FindFirstValue("FullName") ?? user.Identity?.Name;

                    string? userRole = null;
                    if (user.IsInRole(RoleConstants.Developer)) userRole = RoleConstants.Developer;
                    else if (user.IsInRole(RoleConstants.SuperAdmin)) userRole = RoleConstants.SuperAdmin;
                    else if (user.IsInRole(RoleConstants.CollegeCoAdmin)) userRole = RoleConstants.CollegeCoAdmin;
                    else if (user.IsInRole(RoleConstants.Judge)) userRole = RoleConstants.Judge;
                    else if (user.IsInRole(RoleConstants.Researcher)) userRole = RoleConstants.Researcher;

                    var controller = context.RouteData.Values["controller"]?.ToString() ?? "Unknown";
                    var action = context.RouteData.Values["action"]?.ToString() ?? "Unknown";
                    var httpMethod = httpContext.Request.Method;
                    var ip = httpContext.Connection.RemoteIpAddress?.ToString();

                    int statusCode = httpContext.Response.StatusCode;
                    if (caughtException != null)
                    {
                        statusCode = 500;
                    }

                    bool isSuccess = caughtException == null && statusCode < 400;

                    var description = GenerateActionDescription(controller, action, httpMethod, isSuccess);

                    var auditLog = new AuditLog
                    {
                        UserId = userId,
                        UserEmail = userEmail,
                        UserName = userName,
                        UserRole = userRole,
                        ControllerName = controller,
                        ActionName = action,
                        HttpMethod = httpMethod,
                        Path = path,
                        IpAddress = ip,
                        StatusCode = statusCode,
                        IsSuccess = isSuccess,
                        Description = description,
                        ExceptionDetails = caughtException?.ToString(),
                        ExecutionDurationMs = stopwatch.ElapsedMilliseconds,
                        Timestamp = DateTime.UtcNow
                    };

                    db.AuditLogs.Add(auditLog);
                    await db.SaveChangesAsync();
                }
                catch (Exception logEx)
                {
                    _logger.LogError(logEx, "فشل حفظ سجل النشاط (Audit Log) في قاعدة البيانات.");
                }
            }
        }

        private static string GenerateActionDescription(string controller, string action, string method, bool isSuccess)
        {
            var prefix = isSuccess ? "تم بنجاح: " : "محاولة: ";
            return controller.ToLowerInvariant() switch
            {
                "account" => action.ToLowerInvariant() switch
                {
                    "login" => prefix + (isSuccess ? "تسجيل الدخول إلى النظام" : "فشل تسجيل الدخول"),
                    "logout" => "تسجيل الخروج من المنصة",
                    "register" => prefix + "تسجيل حساب باحث جديد",
                    "verifycode" or "verifyemail" => prefix + "التحقق من رمز تأكيد البريد الإلكتروني",
                    "forgotpassword" => prefix + "طلب استعادة كلمة المرور",
                    "resetpassword" => prefix + "إعادة تعيين كلمة المرور",
                    "acceptjudgeinvitation" => prefix + "قبول دعوة الانضمام كمحكم",
                    _ => $"{prefix}عملية حساب المستخدم ({action})"
                },
                "researcher" => action.ToLowerInvariant() switch
                {
                    "submitresearch" or "createresearch" => prefix + "تقديم مقترح بحثي جديد",
                    "modifyresearch" => prefix + "إعادة تقديم البحث بعد التعديل",
                    "details" => "استعراض تفاصيل البحث",
                    _ => $"{prefix}إجراءات الباحث ({action})"
                },
                "collegecoadmin" => action.ToLowerInvariant() switch
                {
                    "invitejudge" => prefix + "إرسال دعوة لمحكم خارجي",
                    "assignjudges" => prefix + "إسناد أبحاث للتحكيم",
                    "finalapproval" or "issuecertificate" => prefix + "إصدار وتوثيق شهادة أخلاقيات البحث",
                    "formalscreening" => prefix + "الفحص الشكلي للبحث",
                    _ => $"{prefix}إجراءات مشرف الكلية ({action})"
                },
                "judge" => action.ToLowerInvariant() switch
                {
                    "submitevaluation" => prefix + "إرسال تقييم وقرار التحكيم",
                    "declareconflictofinterest" => prefix + "إقرار تعارض المصالح والاعتذار عن التحكيم",
                    _ => $"{prefix}إجراءات المحكم ({action})"
                },
                "superadmin" => action.ToLowerInvariant() switch
                {
                    "createcoadminnewuser" => prefix + "إنشاء حساب مشرف كلية جديد",
                    "addcoadminroletoexistinguser" => prefix + "إسناد دور مشرف كلية لمستخدم",
                    "savecollege" or "createcollege" => prefix + "إضافة/تعديل كلية",
                    "savedepartment" => prefix + "إضافة/تعديل قسم علمي",
                    "savecategory" => prefix + "إضافة/تعديل تصنيف بحثي",
                    _ => $"{prefix}إجراءات المشرف العام ({action})"
                },
                "developer" => action.ToLowerInvariant() switch
                {
                    "auditlogs" or "auditlogspartial" => "استعراض سجلات الرقابة والتدقيق بالنظام",
                    "hangfire" => "استعراض لوحة مهام Hangfire",
                    _ => $"{prefix}إجراءات مهندس التطوير ({action})"
                },
                "attachment" => action.ToLowerInvariant() switch
                {
                    "download" => prefix + "تحميل ملف مرفق",
                    _ => $"{prefix}عملية على المرفقات ({action})"
                },
                _ => $"{prefix}طلب {method} على المسار {controller}/{action}"
            };
        }
    }
}

