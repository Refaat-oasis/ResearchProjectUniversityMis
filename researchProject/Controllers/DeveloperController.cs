using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using researchProject.Models.Constants;
using researchProject.Repositories.Interfaces;

namespace researchProject.Controllers
{
    [Authorize(Roles = RoleConstants.Developer)]
    public class DeveloperController : Controller
    {
        private readonly ISuperAdminRepository _superAdminRepo;

        public DeveloperController(ISuperAdminRepository superAdminRepo)
        {
            _superAdminRepo = superAdminRepo;
        }

        [HttpGet]
        public async Task<IActionResult> AuditLogs(
            int page = 1,
            string? searchTerm = null,
            string? roleFilter = null,
            bool? successFilter = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var model = await _superAdminRepo.GetAuditLogsAsync(page, 25, searchTerm, roleFilter, successFilter, fromDate, toDate);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AuditLogsPartial(
            int page = 1,
            string? searchTerm = null,
            string? roleFilter = null,
            bool? successFilter = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var model = await _superAdminRepo.GetAuditLogsAsync(page, 25, searchTerm, roleFilter, successFilter, fromDate, toDate);
            return PartialView("_AuditLogsPartial", model);
        }

        [HttpGet]
        public IActionResult Hangfire()
        {
            return View();
        }
    }
}
