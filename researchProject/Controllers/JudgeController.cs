using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using researchProject.Models.Constants;
using researchProject.Models.DTOs;
using researchProject.Repositories.Interfaces;

namespace researchProject.Controllers
{
    [Authorize(Policy = "JudgeOrSuperAdmin")]
    public class JudgeController : Controller
    {
        private readonly IJudgeRepository _judgeRepo;

        public JudgeController(IJudgeRepository judgeRepo)
        {
            _judgeRepo = judgeRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var profile = await _judgeRepo.GetProfileByUserIdAsync(userId);
            ViewBag.Profile = profile;
            ViewBag.InitialEvaluations = await _judgeRepo.GetAssignedEvaluationsAsync(userId);
            return View(profile);
        }

        [HttpGet]
        public async Task<IActionResult> AssignedResearchesPartial()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var evaluations = await _judgeRepo.GetAssignedEvaluationsAsync(userId);
            return PartialView("_AssignedResearchesPartial", evaluations);
        }

        [HttpGet]
        public async Task<IActionResult> EvaluationFormPartial(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var evaluation = await _judgeRepo.GetEvaluationDetailsAsync(id, userId);
            if (evaluation == null)
            {
                return NotFound("طلب التحكيم غير موجود أو غير مسند إليك، أو يوجد تعارض مصالح يمنعك من تحكيمه.");
            }

            return PartialView("_EvaluationFormPartial", evaluation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitEvaluation(EvaluationSubmitDto dto)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "يرجى استيفاء جميع تقييمات المعايير الأخلاقية الإلزامية وقرار التحكيم." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var (succeeded, errorMessage) = await _judgeRepo.SubmitEvaluationAsync(dto, userId);

            return Json(new 
            { 
                success = succeeded, 
                message = errorMessage ?? "تم تقديم التقييم الأخلاقي وتوقيعه رقمياً بنجاح." 
            });
        }
    }
}
