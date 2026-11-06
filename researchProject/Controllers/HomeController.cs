using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using researchProject.Models;
using researchProject.Models.Constants;

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

            return RedirectToAction("Login", "Account");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
