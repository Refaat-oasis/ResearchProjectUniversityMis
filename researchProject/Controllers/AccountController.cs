using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using researchProject.Models.Constants;
using researchProject.Models.DTOs;
using researchProject.Models.ViewModels;
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
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                var vm = new LoginViewModel { Email = dto.Email, RememberMe = dto.RememberMe, ReturnUrl = returnUrl };
                return View(vm);
            }

            var (succeeded, isLockedOut, requiresTwoFactor, errorMessage, roles) = 
                await _accountRepository.LoginAsync(dto.Email, dto.Password, dto.RememberMe);

            if (isLockedOut)
            {
                ModelState.AddModelError(string.Empty, "تم قفل الحساب مؤقتاً بسبب تكرار المحاولات الخاطئة. يرجى المحاولة بعد 15 دقيقة.");
                var vm = new LoginViewModel { Email = dto.Email, RememberMe = dto.RememberMe, ReturnUrl = returnUrl };
                return View(vm);
            }

            if (!succeeded)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "فشل تسجيل الدخول. يرجى التحقق من صحة البيانات.");
                var vm = new LoginViewModel { Email = dto.Email, RememberMe = dto.RememberMe, ReturnUrl = returnUrl, ErrorMessage = errorMessage };
                return View(vm);
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

            var colleges = await _accountRepository.GetActiveCollegesAsync();
            ViewBag.Colleges = colleges;
            return View(new ResearcherSignUpViewModel { AvailableColleges = colleges });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(ResearcherSignUpDto dto)
        {
            if (!ModelState.IsValid)
            {
                var colleges = await _accountRepository.GetActiveCollegesAsync();
                ViewBag.Colleges = colleges;
                var vm = new ResearcherSignUpViewModel
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    CollegeId = dto.CollegeId,
                    ResearcherType = dto.ResearcherType,
                    AcademicDegree = dto.AcademicDegree,
                    Department = dto.Department,
                    NationalId = dto.NationalId,
                    PhoneNumber = dto.PhoneNumber,
                    AvailableColleges = colleges
                };
                return View(vm);
            }

            var (succeeded, errorMessage, confirmationToken, userId) = await _accountRepository.SignUpResearcherAsync(dto);

            if (!succeeded)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "فشل تسجيل الحساب.");
                var colleges = await _accountRepository.GetActiveCollegesAsync();
                ViewBag.Colleges = colleges;
                var vm = new ResearcherSignUpViewModel
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    CollegeId = dto.CollegeId,
                    ResearcherType = dto.ResearcherType,
                    AcademicDegree = dto.AcademicDegree,
                    Department = dto.Department,
                    NationalId = dto.NationalId,
                    PhoneNumber = dto.PhoneNumber,
                    AvailableColleges = colleges,
                    ErrorMessage = errorMessage
                };
                return View(vm);
            }

            // Generate direct confirmation link
            var confirmationLink = Url.Action(
                nameof(VerifyEmailToken), 
                "Account", 
                new { email = dto.Email, token = confirmationToken }, 
                Request.Scheme);

            if (!string.IsNullOrEmpty(confirmationLink) && !string.IsNullOrEmpty(confirmationToken))
            {
                await _emailSender.SendRegistrationTokenAsync(dto.Email, confirmationToken, confirmationLink);
            }

            TempData["SuccessMessage"] = "تم إنشاء الحساب بنجاح! تم إرسال رمز التحقق الرقمي (صالح لمدة 15 دقيقة) إلى بريدك الإلكتروني. يرجى تفعيل الحساب للمتابعة.";
            return RedirectToAction(nameof(VerifyEmailToken), new { email = dto.Email, userId = userId });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetDepartmentsByCollege(int collegeId)
        {
            var departments = await _accountRepository.GetDepartmentsByCollegeAsync(collegeId);
            return Json(departments.Select(d => new { id = d.Id, name = d.Name }));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(new ForgotPasswordViewModel { Email = dto.Email });
            }

            var (succeeded, errorMessage, resetToken, userId) = await _accountRepository.GeneratePasswordResetTokenAsync(dto.Email);
            if (succeeded && !string.IsNullOrEmpty(resetToken))
            {
                var resetLink = Url.Action(nameof(ResetPassword), "Account", new { email = dto.Email, token = resetToken }, Request.Scheme);
                if (!string.IsNullOrEmpty(resetLink))
                {
                    await _emailSender.SendPasswordResetTokenAsync(dto.Email, resetToken, resetLink);
                }
            }

            TempData["SuccessMessage"] = "إذا كان بريدك الإلكتروني مسجلاً في النظام، فقد تم إرسال رمز ورابط استعادة كلمة المرور إليه.";
            return RedirectToAction(nameof(ResetPassword), new { email = dto.Email });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string? email = null, string? token = null)
        {
            var model = new ResetPasswordViewModel
            {
                Email = email ?? string.Empty,
                Token = token ?? string.Empty
            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(new ResetPasswordViewModel { Email = dto.Email, Token = dto.Token });
            }

            var (succeeded, errorMessage) = await _accountRepository.ResetPasswordWithTokenAsync(dto.Email, dto.Token, dto.NewPassword);
            if (!succeeded)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "فشل إعادة تعيين كلمة المرور.");
                return View(new ResetPasswordViewModel { Email = dto.Email, Token = dto.Token, ErrorMessage = errorMessage });
            }

            TempData["SuccessMessage"] = "تم تغيير كلمة المرور بنجاح! يمكنك الآن تسجيل الدخول بكلمة المرور الجديدة.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmailToken(string? email = null, string? userId = null, string? token = null)
        {
            var identifier = !string.IsNullOrEmpty(email) ? email : userId;

            if (!string.IsNullOrEmpty(identifier) && !string.IsNullOrEmpty(token))
            {
                var (succeeded, errorMessage) = await _accountRepository.ConfirmEmailWithTokenOrCodeAsync(identifier, token);
                if (succeeded)
                {
                    TempData["SuccessMessage"] = "تم تأكيد بريدك الإلكتروني بنجاح! يمكنك الآن تسجيل الدخول إلى المنصة.";
                    return RedirectToAction(nameof(Login));
                }
                else
                {
                    ViewBag.ErrorMessage = errorMessage;
                }
            }

            var model = new VerifyEmailViewModel
            {
                EmailOrUserId = identifier ?? string.Empty,
                Token = token ?? string.Empty,
                ValidityMinutes = 15
            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmailToken(VerifyEmailDto dto)
        {
            if (!ModelState.IsValid)
            {
                var vm = new VerifyEmailViewModel
                {
                    EmailOrUserId = dto.EmailOrUserId,
                    Token = dto.Token,
                    ValidityMinutes = 15
                };
                return View(vm);
            }

            var (succeeded, errorMessage) = await _accountRepository.ValidateVerificationCodeAsync(dto.EmailOrUserId, dto.Token, "EmailConfirmation");
            if (!succeeded)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "رمز التحقق غير صالح أو منتهي الصلاحية.");
                var vm = new VerifyEmailViewModel
                {
                    EmailOrUserId = dto.EmailOrUserId,
                    Token = dto.Token,
                    ValidityMinutes = 15,
                    ErrorMessage = errorMessage
                };
                return View(vm);
            }

            TempData["SuccessMessage"] = "تم تأكيد بريدك الإلكتروني بنجاح! يمكنك الآن تسجيل الدخول إلى المنصة.";
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerificationCode(ResendVerificationCodeDto dto)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.EmailOrUserId))
            {
                return Json(new { success = false, message = "يرجى إدخال البريد الإلكتروني لإعادة الإرسال." });
            }

            var (succeeded, errorMessage, newCode, resolvedEmail) = await _accountRepository.ResendVerificationCodeAsync(dto.EmailOrUserId, "EmailConfirmation");
            if (!succeeded || string.IsNullOrEmpty(newCode))
            {
                return Json(new { success = false, message = errorMessage ?? "فشل إعادة إرسال رمز التحقق." });
            }

            var emailToUse = resolvedEmail ?? dto.EmailOrUserId;
            var confirmationLink = Url.Action(
                nameof(VerifyEmailToken),
                "Account",
                new { email = emailToUse, token = newCode },
                Request.Scheme);

            if (!string.IsNullOrEmpty(confirmationLink))
            {
                await _emailSender.SendRegistrationTokenAsync(emailToUse, newCode, confirmationLink);
            }

            return Json(new { success = true, message = "تم إرسال رمز تحقق جديد مكوّن من 6 أرقام إلى بريدك الإلكتروني (صالح لمدة 15 دقيقة)." });
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

            var vm = new AcceptInvitationViewModel
            {
                Token = token,
                Email = invitation.Email,
                Specialty = invitation.Specialty
            };

            // If user is logged in
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var (succeeded, errorMessage) = await _accountRepository.AcceptJudgeInvitationAsync(token, null, null, null, userId);

                if (succeeded)
                {
                    if (!string.IsNullOrEmpty(userId))
                    {
                        await _accountRepository.RefreshSignInAsync(userId);
                    }

                    TempData["SuccessMessage"] = "تم قبول دعوة التحكيم وإضافة صفة محكم إلى حسابك الحالي بنجاح!";
                    return RedirectToAction("AssignedResearches", "Judge");
                }

                TempData["ErrorMessage"] = errorMessage ?? "تعذر قبول الدعوة.";
                return RedirectToAction(nameof(Login));
            }

            ViewBag.InvitationEmail = invitation.Email;
            ViewBag.Specialty = invitation.Specialty;
            return View(vm);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptJudgeInvitation(AcceptInvitationDto dto)
        {
            if (!ModelState.IsValid)
            {
                var vm = new AcceptInvitationViewModel
                {
                    Token = dto.Token,
                    FullName = dto.FullName,
                    AcademicTitle = dto.AcademicTitle
                };
                return View(vm);
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
                var vm = new AcceptInvitationViewModel
                {
                    Token = dto.Token,
                    FullName = dto.FullName,
                    AcademicTitle = dto.AcademicTitle,
                    ErrorMessage = errorMessage
                };
                return View(vm);
            }

            TempData["SuccessMessage"] = "تم تفعيل حساب المحكم بنجاح! يمكنك الآن تسجيل الدخول ببياناتك.";
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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(nameof(Login));
            }

            var vm = await _accountRepository.GetProfileViewModelAsync(userId);
            if (vm == null)
            {
                return NotFound("تعذر العثور على الملف الشخصي للمستخدم.");
            }

            return View(vm);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("، ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { success = false, message = errors });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "غير مصرح." });
            }

            var (succeeded, errorMessage) = await _accountRepository.UpdateProfileAsync(userId, dto);
            return Json(new { success = succeeded, message = succeeded ? "تم حفظ البيانات الشخصية بنجاح." : errorMessage });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("، ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { success = false, message = errors });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "غير مصرح." });
            }

            var (succeeded, errorMessage) = await _accountRepository.ChangePasswordAsync(userId, dto);
            return Json(new { success = succeeded, message = succeeded ? "تم تحديث كلمة المرور بنجاح." : errorMessage });
        }

        private IActionResult RedirectToUserDashboard()
        {
            if (User.IsInRole(RoleConstants.Developer))
                return RedirectToAction("AuditLogs", "Developer");
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
            if (roles.Contains(RoleConstants.Developer))
                return RedirectToAction("AuditLogs", "Developer");
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

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ProfilePartial()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var profile = await _accountRepository.GetUserProfileAsync(userId);
            if (profile == null) return NotFound("المستخدم غير موجود.");

            return PartialView("_UserProfilePartial", profile);
        }

        [Authorize(Roles = RoleConstants.Judge)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BecomeResearcher([FromBody] JudgeToResearcherDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("، ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { success = false, message = errors });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var (succeeded, errorMessage) = await _accountRepository.AddResearcherRoleToJudgeAsync(userId, dto);
            if (!succeeded)
            {
                return Json(new { success = false, message = errorMessage });
            }

            // Refresh user claims & cookies immediately
            await _accountRepository.RefreshSignInAsync(userId);

            return Json(new { success = true, message = "تهانينا! تم تفعيل دور الباحث الأكاديمي لحسابك بنجاح. يمكنك الآن تقديم مقترحاتك البحثية من بوابتك الأكاديمية." });
        }
    }
}
