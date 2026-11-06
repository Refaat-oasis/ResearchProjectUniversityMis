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
            // 1. Institutional Domain Verification
            var allowedDomains = _configuration.GetSection("Auth:AllowedEmailDomains").Get<string[]>() ?? new[] { "univ.edu.eg", "mis.edu.eg" };
            var emailDomain = dto.Email.Split('@').LastOrDefault()?.ToLowerInvariant();

            if (string.IsNullOrEmpty(emailDomain) || !allowedDomains.Any(d => d.Equals(emailDomain, StringComparison.OrdinalIgnoreCase)))
            {
                var formattedDomains = string.Join("، ", allowedDomains.Select(d => $"@{d}"));
                return (false, $"التسجيل متاح فقط بالبريد الإلكتروني الأكاديمي والمؤسسي المعتمد للجامعة ({formattedDomains}).", null, null);
            }

            // 2. Check existing user
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return (false, "البريد الإلكتروني مسجل مسبقاً في النظام.", null, null);
            }

            // 3. Check College exists
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

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                return (true, null, token, user.Id);
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

        private static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
