using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using researchProject.Models.Constants;

namespace researchProject.Services
{
    public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            if (httpContext == null || httpContext.User == null)
            {
                return false;
            }

            if (!httpContext.User.Identity?.IsAuthenticated ?? true)
            {
                return false;
            }

            return httpContext.User.IsInRole(RoleConstants.SuperAdmin) || httpContext.User.IsInRole(RoleConstants.Developer);
        }
    }
}

