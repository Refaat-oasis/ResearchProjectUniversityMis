using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using researchProject.Models.Constants;
using researchProject.Models.DTOs;
using researchProject.Repositories.Interfaces;
using researchProject.Services;

namespace researchProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IEmailSender _emailSender;

        public AccountController(IAccountRepository accountRepository, IEmailSender emailSender)
        {
            _accountRepository = accountRepository;
            _emailSender = emailSender;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToUserDashboard();
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginDto());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var (succeeded, isLockedOut, requiresTwoFactor, errorMessage, roles) = 
                await _accountRepository.LoginAsync(dto.Email, dto.Password, dto.RememberMe);

            if (isLockedOut)
            {
                ModelState.AddModelError(string.Empty, "تم قفل الحساب مؤقتاً بسبب تكرار المحاولات الخاطئة. يرجى المحاولة بعد 15 دقيقة.");
                return View(dto);
            }

            if (!succeeded)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "فشل تسجيل الدخول. يرجى التحقق من صحة البيانات.");
                return View(dto);
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToRoleDashboard(roles);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SignUp()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToUserDashboard();
            }

            ViewBag.Colleges = await _accountRepository.GetActiveCollegesAsync();
            return View(new ResearcherSignUpDto());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(ResearcherSignUpDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Colleges = await _accountRepository.GetActiveCollegesAsync();
                return View(dto);
            }

            var (succeeded, errorMessage, confirmationToken, userId) = await _accountRepository.SignUpResearcherAsync(dto);

            if (!succeeded)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "فشل تسجيل الحساب.");
                ViewBag.Colleges = await _accountRepository.GetActiveCollegesAsync();
                return View(dto);
            }

            // Generate confirmation link
            var confirmationLink = Url.Action(
                nameof(ConfirmEmail), 
                "Account", 
                new { userId = userId, token = confirmationToken }, 
                Request.Scheme);

            if (!string.IsNullOrEmpty(confirmationLink))
            {
                await _emailSender.SendEmailConfirmationAsync(dto.Email, confirmationLink);
            }

            TempData["SuccessMessage"] = "تم إنشاء الحساب بنجاح! تم إرسال رابط تأكيد الحساب إلى بريدك الإلكتروني المؤسسي. يرجى فتح البريد وتأكيد الحساب قبل تسجيل الدخول.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "رابط تأكيد الحساب غير صحيح.";
                return RedirectToAction(nameof(Login));
            }

            var (succeeded, errorMessage) = await _accountRepository.ConfirmEmailAsync(userId, token);
            if (succeeded)
            {
                TempData["SuccessMessage"] = "تم تأكيد بريدك الإلكتروني بنجاح! يمكنك الآن تسجيل الدخول إلى المنصة.";
            }
            else
            {
                TempData["ErrorMessage"] = errorMessage ?? "فشل تأكيد البريد الإلكتروني أو انتهت صلاحية الرابط.";
            }

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> AcceptJudgeInvitation(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "رابط الدعوة غير صالح.";
                return RedirectToAction(nameof(Login));
            }

            var invitation = await _accountRepository.GetInvitationByTokenAsync(token);
            if (invitation == null)
            {
                TempData["ErrorMessage"] = "الدعوة غير صالحة أو تم استخدامها مسبقاً أو انتهت مدة صلاحيتها (7 أيام).";
                return RedirectToAction(nameof(Login));
            }

            var dto = new AcceptInvitationDto
            {
                Token = token
            };

            // If user is logged in
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var (succeeded, errorMessage) = await _accountRepository.AcceptJudgeInvitationAsync(token, null, null, null, userId);

                if (succeeded)
                {
                    TempData["SuccessMessage"] = "تم قبول دعوة التحكيم وإضافة صفة محكم إلى حسابك الحالي بنجاح!";
                    return RedirectToAction(nameof(Login));
                }

                TempData["ErrorMessage"] = errorMessage ?? "تعذر قبول الدعوة.";
                return RedirectToAction(nameof(Login));
            }

            ViewBag.InvitationEmail = invitation.Email;
            ViewBag.Specialty = invitation.Specialty;
            return View(dto);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptJudgeInvitation(AcceptInvitationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var (succeeded, errorMessage) = await _accountRepository.AcceptJudgeInvitationAsync(
                dto.Token, 
                dto.FullName, 
                dto.Password, 
                dto.AcademicTitle, 
                null);

            if (!succeeded)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "تعذر إكمال قبول الدعوة وإنشاء الحساب.");
                return View(dto);
            }

            TempData["SuccessMessage"] = "تم إنشاء وتفعيل حساب المحكم بنجاح! يمكنك الآن تسجيل الدخول ببياناتك الجديدة.";
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _accountRepository.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToUserDashboard()
        {
            if (User.IsInRole(RoleConstants.SuperAdmin))
                return RedirectToAction("Index", "SuperAdmin");
            if (User.IsInRole(RoleConstants.CollegeCoAdmin))
                return RedirectToAction("Index", "CollegeCoAdmin");
            if (User.IsInRole(RoleConstants.Judge))
                return RedirectToAction("Index", "Judge");
            if (User.IsInRole(RoleConstants.Researcher))
                return RedirectToAction("Index", "Researcher");

            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToRoleDashboard(IList<string> roles)
        {
            if (roles.Contains(RoleConstants.SuperAdmin))
                return RedirectToAction("Index", "SuperAdmin");
            if (roles.Contains(RoleConstants.CollegeCoAdmin))
                return RedirectToAction("Index", "CollegeCoAdmin");
            if (roles.Contains(RoleConstants.Judge))
                return RedirectToAction("Index", "Judge");
            if (roles.Contains(RoleConstants.Researcher))
                return RedirectToAction("Index", "Researcher");

            return RedirectToAction("Index", "Home");
        }
    }
}
