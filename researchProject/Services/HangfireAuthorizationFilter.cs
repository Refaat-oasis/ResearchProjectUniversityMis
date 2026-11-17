using Hangfire.Dashboard;
using researchProject.Models.Constants;

namespace researchProject.Services
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            if (httpContext?.User == null || !httpContext.User.Identity?.IsAuthenticated == true)
            {
                return false;
            }

            return httpContext.User.IsInRole(RoleConstants.SuperAdmin);
        }
    }
}

