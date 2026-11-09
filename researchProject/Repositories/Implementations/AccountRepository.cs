using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using researchProject.Data;
using researchProject.Models.Constants;
using researchProject.Models.DTOs;
using researchProject.Models.Entities;
using researchProject.Repositories.Interfaces;
using researchProject.Services;

namespace researchProject.Repositories.Implementations
{
    public class AccountRepository : IAccountRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;

        public AccountRepository(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db,
            IConfiguration configuration,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _db = db;
            _configuration = configuration;
            _emailSender = emailSender;
        }

        public async Task<(bool Succeeded, bool IsLockedOut, bool RequiresTwoFactor, string? ErrorMessage, IList<string> Roles)> LoginAsync(string email, string password, bool rememberMe)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return (false, false, false, "البريد الإلكتروني أو كلمة المرور غير صحيحة", new List<string>());
            }

            if (!user.EmailConfirmed)
            {
                return (false, false, false, "يرجى تأكيد بريدك الإلكتروني من خلال الرابط المرسل إليك قبل تسجيل الدخول.", new List<string>());
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName ?? email, password, rememberMe, lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                return (false, true, false, "تم قفل الحساب مؤقتاً بسبب تكرار محاولات تسجيل الدخول الخاطئة. يرجى المحاولة لاحقاً.", new List<string>());
            }

            if (result.RequiresTwoFactor)
            {
                return (false, false, true, null, new List<string>());
            }

            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);
                return (true, false, false, null, roles);
            }

            return (false, false, false, "البريد الإلكتروني أو كلمة المرور غير صحيحة.", new List<string>());
        }

        public async Task<(bool Succeeded, string? ErrorMessage, string? ConfirmationToken, string? UserId)> SignUpResearcherAsync(ResearcherSignUpDto dto)
        {
            // 1. Check existing user
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return (false, "البريد الإلكتروني مسجل مسبقاً في النظام.", null, null);
            }

            // 2. Check College exists
            var collegeExists = await _db.Colleges.AnyAsync(c => c.Id == dto.CollegeId);
            if (!collegeExists)
            {
                return (false, "الكلية المحددة غير موجودة بالنظام.", null, null);
            }

            // 4. Single Transaction for User, Role, and Researcher Profile
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FullName = dto.FullName,
                    PhoneNumber = dto.PhoneNumber,
                    EmailConfirmed = false
                };

                var createResult = await _userManager.CreateAsync(user, dto.Password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("، ", createResult.Errors.Select(e => e.Description));
                    return (false, $"فشل إنشاء الحساب: {errors}", null, null);
                }

                // Ensure role exists and assign
                if (!await _roleManager.RoleExistsAsync(RoleConstants.Researcher))
                {
                    await _roleManager.CreateAsync(new IdentityRole(RoleConstants.Researcher));
                }
                await _userManager.AddToRoleAsync(user, RoleConstants.Researcher);

                // Create Researcher Profile
                var researcher = new Researcher
                {
                    UserId = user.Id,
                    CollegeId = dto.CollegeId,
                    ResearcherType = dto.ResearcherType,
                    AcademicDegree = dto.AcademicDegree,
                    Department = dto.Department,
                    NationalId = dto.NationalId
                };

                await _db.Researchers.AddAsync(researcher);
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();

                // Generate 6-digit verification code with 15-minute expiration
                var (otpOk, otpErr, otpCode, _) = await GenerateVerificationCodeAsync(user.Email!, "EmailConfirmation", user.Id);
                return (true, null, otpCode, user.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"حدث خطأ أثناء إنشاء الحساب: {ex.Message}", null, null);
            }
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, "المستخدم غير موجود.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return (true, null);
            }

            return (false, "رابط التأكيد غير صالح أو منتهي الصلاحية.");
        }

        public async Task<JudgeInvitation?> GetInvitationByTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;
            var hash = HashToken(token);

            var invitation = await _db.JudgeInvitations
                .Include(i => i.InvitedByUser)
                .FirstOrDefaultAsync(i => i.TokenHash == hash);

            if (invitation == null) return null;
            if (invitation.Status != InvitationStatusConstants.Pending || invitation.ExpiresAt < DateTime.UtcNow)
            {
                return null;
            }

            return invitation;
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> AcceptJudgeInvitationAsync(string token, string? fullName, string? password, string? academicTitle, string? signedInUserId)
        {
            if (string.IsNullOrWhiteSpace(token))
                return (false, "الرمز الأمني للدعوة غير صالح.");

            var hash = HashToken(token);
            var invitation = await _db.JudgeInvitations.FirstOrDefaultAsync(i => i.TokenHash == hash);

            if (invitation == null || invitation.Status != InvitationStatusConstants.Pending || invitation.ExpiresAt < DateTime.UtcNow)
            {
                return (false, "الدعوة غير صالحة، أو تم استخدامها مسبقاً، أو انتهت صلاحيتها.");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                ApplicationUser? user = null;

                if (!string.IsNullOrEmpty(signedInUserId))
                {
                    // Existing User flow
                    user = await _userManager.FindByIdAsync(signedInUserId);
                    if (user == null)
                    {
                        return (false, "تعذر العثور على حساب المستخدم الحالي.");
                    }

                    if (!user.Email!.Equals(invitation.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        return (false, "البريد الإلكتروني للحساب الحالي لا يطابق البريد المسجل في الدعوة.");
                    }
                }
                else
                {
                    // New User flow (or unauthenticated lookup)
                    user = await _userManager.FindByEmailAsync(invitation.Email);
                    if (user == null)
                    {
                        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fullName))
                        {
                            return (false, "الاسم الكامل وكلمة المرور مطلوبان لإنشاء حساب المحكم.");
                        }

                        user = new ApplicationUser
                        {
                            UserName = invitation.Email,
                            Email = invitation.Email,
                            FullName = fullName,
                            EmailConfirmed = true // accepting invitation proves email ownership
                        };

                        var createRes = await _userManager.CreateAsync(user, password);
                        if (!createRes.Succeeded)
                        {
                            var errs = string.Join("، ", createRes.Errors.Select(e => e.Description));
                            return (false, $"فشل إنشاء حساب المحكم: {errs}");
                        }
                    }
                }

                // Add Judge Role if not already in role
                if (!await _userManager.IsInRoleAsync(user, RoleConstants.Judge))
                {
                    await _userManager.AddToRoleAsync(user, RoleConstants.Judge);
                }

                // Create or activate Judge Profile
                var judgeProfile = await _db.Judges.FirstOrDefaultAsync(j => j.UserId == user.Id);
                if (judgeProfile == null)
                {
                    judgeProfile = new Judge
                    {
                        UserId = user.Id,
                        Specialty = invitation.Specialty ?? "عام",
                        AcademicTitle = academicTitle ?? "خبير ومحكم علمي",
                        IsActive = true
                    };
                    await _db.Judges.AddAsync(judgeProfile);
                }
                else
                {
                    judgeProfile.IsActive = true;
                    if (!string.IsNullOrEmpty(invitation.Specialty))
                    {
                        judgeProfile.Specialty = invitation.Specialty;
                    }
                }

                // Mark invitation accepted
                invitation.Status = InvitationStatusConstants.Accepted;
                invitation.AcceptedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"حدث خطأ أثناء قبول الدعوة: {ex.Message}");
            }
        }

        public async Task RefreshSignInAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return;
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _signInManager.RefreshSignInAsync(user);
            }
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<ApplicationUser?> FindByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<ApplicationUser?> FindByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<List<College>> GetActiveCollegesAsync()
        {
            return await _db.Colleges.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<List<Department>> GetDepartmentsByCollegeAsync(int collegeId)
        {
            return await _db.Departments
                .Where(d => d.CollegeId == collegeId)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<(bool Succeeded, string? ErrorMessage, string? ResetToken, string? UserId)> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return (false, "البريد الإلكتروني غير مسجل بالنظام.", null, null);
            }

            var (ok, err, code, _) = await GenerateVerificationCodeAsync(user.Email!, "PasswordReset", user.Id);
            return (ok, err, code, user.Id);
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> ResetPasswordWithTokenAsync(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return (false, "البريد الإلكتروني غير مسجل بالنظام.");
            }

            var cleanToken = token.Trim();
            if (cleanToken.Length == 6 && int.TryParse(cleanToken, out _))
            {
                var (valid, validErr) = await ValidateVerificationCodeAsync(email, cleanToken, "PasswordReset");
                if (!valid)
                {
                    return (false, validErr ?? "رمز استعادة كلمة المرور غير صالح أو منتهي الصلاحية.");
                }

                var internalResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await _userManager.ResetPasswordAsync(user, internalResetToken, newPassword);
                if (resetResult.Succeeded)
                {
                    return (true, null);
                }

                var errors = string.Join("، ", resetResult.Errors.Select(e => e.Description));
                return (false, $"فشل إعادة تعيين كلمة المرور: {errors}");
            }
            else
            {
                // Fallback for raw Identity token
                var result = await _userManager.ResetPasswordAsync(user, cleanToken, newPassword);
                if (result.Succeeded)
                {
                    return (true, null);
                }

                var errors = string.Join("، ", result.Errors.Select(e => e.Description));
                return (false, $"فشل إعادة تعيين كلمة المرور: {errors}");
            }
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> ConfirmEmailWithTokenOrCodeAsync(string emailOrUserId, string token)
        {
            if (string.IsNullOrWhiteSpace(emailOrUserId) || string.IsNullOrWhiteSpace(token))
            {
                return (false, "البيانات المدخلة غير مكتملة.");
            }

            var cleanToken = token.Trim();
            if (cleanToken.Length == 6 && int.TryParse(cleanToken, out _))
            {
                return await ValidateVerificationCodeAsync(emailOrUserId, cleanToken, "EmailConfirmation");
            }

            // Fallback for direct token
            ApplicationUser? user = null;
            if (emailOrUserId.Contains('@'))
            {
                user = await _userManager.FindByEmailAsync(emailOrUserId);
            }
            else
            {
                user = await _userManager.FindByIdAsync(emailOrUserId);
            }

            if (user == null)
            {
                return (false, "تعذر العثور على حساب المستخدم.");
            }

            if (user.EmailConfirmed)
            {
                return (true, null);
            }

            var result = await _userManager.ConfirmEmailAsync(user, cleanToken);
            if (result.Succeeded)
            {
                return (true, null);
            }

            return (false, "رمز التحقق غير صالح أو منتهي الصلاحية.");
        }

        public async Task<(bool Succeeded, string? ErrorMessage, string? Code, string? UserId)> GenerateVerificationCodeAsync(string email, string purpose, string? userId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return (false, "البريد الإلكتروني مطلوب.", null, null);
            }

            var normEmail = email.Trim().ToLowerInvariant();

            // Invalidate existing pending unused codes for this email and purpose
            var pendingCodes = await _db.EmailVerificationCodes
                .Where(c => c.Email == normEmail && c.Purpose == purpose && !c.IsUsed)
                .ToListAsync();

            foreach (var pc in pendingCodes)
            {
                pc.IsUsed = true;
            }

            // Generate cryptographically secure 6-digit numeric OTP
            var code = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var now = DateTime.UtcNow;

            var verificationCode = new EmailVerificationCode
            {
                Email = normEmail,
                UserId = userId,
                Code = code,
                Purpose = purpose,
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(15), // Strictly 15 minutes!
                IsUsed = false,
                AttemptsCount = 0
            };

            await _db.EmailVerificationCodes.AddAsync(verificationCode);
            await _db.SaveChangesAsync();

            return (true, null, code, userId);
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> ValidateVerificationCodeAsync(string emailOrUserId, string code, string purpose)
        {
            if (string.IsNullOrWhiteSpace(emailOrUserId) || string.IsNullOrWhiteSpace(code))
            {
                return (false, "يرجى إدخال البريد الإلكتروني ورمز التحقق.");
            }

            var cleanCode = code.Trim();
            ApplicationUser? user = null;
            string email = emailOrUserId.Trim();

            if (email.Contains('@'))
            {
                user = await _userManager.FindByEmailAsync(email);
            }
            else
            {
                user = await _userManager.FindByIdAsync(emailOrUserId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    email = user.Email;
                }
            }

            var normEmail = email.ToLowerInvariant();
            var now = DateTime.UtcNow;

            var activeCode = await _db.EmailVerificationCodes
                .Where(c => c.Email == normEmail && c.Purpose == purpose && !c.IsUsed)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            if (activeCode == null)
            {
                return (false, "لا يوجد رمز تحقق نشط لهذا البريد. يرجى طلب رمز جديد.");
            }

            if (activeCode.ExpiresAt < now)
            {
                return (false, "انتهت صلاحية رمز التحقق (مدة الصلاحية 15 دقيقة). يرجى طلب إعادة إرسال رمز جديد.");
            }

            if (activeCode.AttemptsCount >= 5)
            {
                activeCode.IsUsed = true;
                await _db.SaveChangesAsync();
                return (false, "تم تجاوز الحد الأقصى للمحاولات الخاطئة (5 محاولات). يرجى طلب رمز جديد.");
            }

            var isCodeMatch = System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(activeCode.Code),
                System.Text.Encoding.UTF8.GetBytes(cleanCode));

            if (!isCodeMatch)
            {
                activeCode.AttemptsCount++;
                if (activeCode.AttemptsCount >= 5)
                {
                    activeCode.IsUsed = true;
                    await _db.SaveChangesAsync();
                    return (false, "تم تجاوز الحد الأقصى للمحاولات الخاطئة (5 محاولات). يرجى طلب رمز جديد.");
                }
                await _db.SaveChangesAsync();
                var remaining = 5 - activeCode.AttemptsCount;
                return (false, $"رمز التحقق غير صحيح. المحاولات المتبقية: {remaining}");
            }

            // Successfully verified!
            activeCode.IsUsed = true;
            activeCode.UsedAt = now;

            if (purpose == "EmailConfirmation" && user != null)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Succeeded, string? ErrorMessage, string? Code)> ResendVerificationCodeAsync(string emailOrUserId, string purpose)
        {
            if (string.IsNullOrWhiteSpace(emailOrUserId))
            {
                return (false, "البريد الإلكتروني مطلوب.", null);
            }

            ApplicationUser? user = null;
            string email = emailOrUserId.Trim();

            if (email.Contains('@'))
            {
                user = await _userManager.FindByEmailAsync(email);
            }
            else
            {
                user = await _userManager.FindByIdAsync(emailOrUserId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    email = user.Email;
                }
            }

            var normEmail = email.ToLowerInvariant();

            if (user == null)
            {
                return (false, "البريد الإلكتروني المدخل غير مسجل في المنصة.", null);
            }

            if (user.EmailConfirmed && purpose == "EmailConfirmation")
            {
                return (false, "هذا الحساب مفعل بالفعل، يمكنك تسجيل الدخول مباشرة.", null);
            }

            // Enforce cooldown (60 seconds)
            var lastCode = await _db.EmailVerificationCodes
                .Where(c => c.Email == normEmail && c.Purpose == purpose)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            if (lastCode != null && (DateTime.UtcNow - lastCode.CreatedAt).TotalSeconds < 60)
            {
                var waitSeconds = 60 - (int)(DateTime.UtcNow - lastCode.CreatedAt).TotalSeconds;
                return (false, $"يرجى الانتظار {waitSeconds} ثانية قبل طلب رمز جديد.", null);
            }

            var (ok, err, code, _) = await GenerateVerificationCodeAsync(normEmail, purpose, user?.Id);
            return (ok, err, code);
        }

        private static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
