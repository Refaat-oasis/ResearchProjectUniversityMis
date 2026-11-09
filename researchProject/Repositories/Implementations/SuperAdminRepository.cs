using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using researchProject.Data;
using researchProject.Models.Constants;
using researchProject.Models.DTOs;
using researchProject.Models.Entities;
using researchProject.Models.ViewModels;
using researchProject.Repositories.Interfaces;
using researchProject.Services;

namespace researchProject.Repositories.Implementations
{
    public class SuperAdminRepository : ISuperAdminRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;

        public SuperAdminRepository(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
        }

        public async Task<List<College>> GetAllCollegesAsync()
        {
            return await _db.Colleges
                .Include(c => c.CollegeCoAdmins).ThenInclude(a => a.User)
                .Include(c => c.ResearchCategories)
                .Include(c => c.Departments)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<College?> GetCollegeByIdAsync(int collegeId)
        {
            return await _db.Colleges
                .Include(c => c.CollegeCoAdmins).ThenInclude(a => a.User)
                .Include(c => c.ResearchCategories)
                .Include(c => c.Departments)
                .FirstOrDefaultAsync(c => c.Id == collegeId);
        }

        public async Task<List<Department>> GetDepartmentsByCollegeAsync(int collegeId)
        {
            return await _db.Departments
                .Where(d => d.CollegeId == collegeId)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> SaveDepartmentAsync(int? id, string name, int collegeId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return (false, "اسم القسم مطلوب.");
            }

            var collegeExists = await _db.Colleges.AnyAsync(c => c.Id == collegeId);
            if (!collegeExists)
            {
                return (false, "الكلية المحددة غير موجودة.");
            }

            if (id.HasValue && id.Value > 0)
            {
                var existing = await _db.Departments.FirstOrDefaultAsync(d => d.Id == id.Value && d.CollegeId == collegeId);
                if (existing == null)
                {
                    return (false, "القسم الأكاديمي غير موجود.");
                }

                if (await _db.Departments.AnyAsync(d => d.Id != id.Value && d.CollegeId == collegeId && d.Name == name.Trim()))
                {
                    return (false, "يوجد قسم أكاديمي بنفس الاسم في هذه الكلية بالفعل.");
                }

                existing.Name = name.Trim();
                await _db.SaveChangesAsync();
                return (true, null);
            }
            else
            {
                if (await _db.Departments.AnyAsync(d => d.CollegeId == collegeId && d.Name == name.Trim()))
                {
                    return (false, "يوجد قسم أكاديمي بنفس الاسم في هذه الكلية بالفعل.");
                }

                var dept = new Department
                {
                    Name = name.Trim(),
                    CollegeId = collegeId
                };
                await _db.Departments.AddAsync(dept);
                await _db.SaveChangesAsync();
                return (true, null);
            }
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> DeleteDepartmentAsync(int id)
        {
            var dept = await _db.Departments.FindAsync(id);
            if (dept == null)
            {
                return (false, "القسم الأكاديمي غير موجود.");
            }

            _db.Departments.Remove(dept);
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> CreateCollegeAsync(College college)
        {
            if (await _db.Colleges.AnyAsync(c => c.Code == college.Code || c.Name == college.Name))
            {
                return (false, "اسم الكلية أو رمزها مسجل مسبقاً.");
            }

            await _db.Colleges.AddAsync(college);
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> UpdateCollegeAsync(College college)
        {
            var existing = await _db.Colleges.FindAsync(college.Id);
            if (existing == null) return (false, "الكلية غير موجودة.");

            if (await _db.Colleges.AnyAsync(c => c.Id != college.Id && (c.Code == college.Code || c.Name == college.Name)))
            {
                return (false, "اسم الكلية أو رمزها مستخدم بالفعل لكلية أخرى.");
            }

            existing.Name = college.Name;
            existing.Code = college.Code;
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<List<ResearchCategory>> GetAllCategoriesAsync()
        {
            return await _db.ResearchCategories
                .Include(c => c.College)
                .OrderBy(c => c.College.Name).ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<ResearchCategory?> GetCategoryByIdAsync(int categoryId)
        {
            return await _db.ResearchCategories
                .Include(c => c.College)
                .FirstOrDefaultAsync(c => c.Id == categoryId);
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> SaveCategoryAsync(ResearchCategory category)
        {
            if (!await _db.Colleges.AnyAsync(c => c.Id == category.CollegeId))
            {
                return (false, "الكلية المحددة غير موجودة.");
            }

            if (category.Id == 0)
            {
                await _db.ResearchCategories.AddAsync(category);
            }
            else
            {
                var existing = await _db.ResearchCategories.FindAsync(category.Id);
                if (existing == null) return (false, "التصنيف غير موجود.");

                existing.Name = category.Name;
                existing.Description = category.Description;
                existing.CollegeId = category.CollegeId;
                existing.FieldsDefinitionJson = category.FieldsDefinitionJson;
            }

            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<List<CollegeCoAdmin>> GetAllCoAdminsAsync()
        {
            return await _db.CollegeCoAdmins
                .Include(a => a.User)
                .Include(a => a.College)
                .OrderBy(a => a.College.Name)
                .ToListAsync();
        }

        public async Task<(bool Succeeded, string? ErrorMessage, string? TempPassword)> CreateCoAdminNewUserAsync(string email, string fullName, int collegeId)
        {
            if (await _userManager.FindByEmailAsync(email) != null)
            {
                return (false, "البريد الإلكتروني مسجل بالفعل. يمكنك إضافة دور مدير الكلية له مباشرة.", null);
            }

            if (!await _db.Colleges.AnyAsync(c => c.Id == collegeId))
            {
                return (false, "الكلية المحددة غير موجودة.", null);
            }

            var tempPassword = $"CoAdmin@{Guid.NewGuid().ToString("N")[..8]}!2026";
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true // Created by SuperAdmin
            };

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var createRes = await _userManager.CreateAsync(user, tempPassword);
                if (!createRes.Succeeded)
                {
                    return (false, string.Join("، ", createRes.Errors.Select(e => e.Description)), null);
                }

                if (!await _roleManager.RoleExistsAsync(RoleConstants.CollegeCoAdmin))
                {
                    await _roleManager.CreateAsync(new IdentityRole(RoleConstants.CollegeCoAdmin));
                }
                await _userManager.AddToRoleAsync(user, RoleConstants.CollegeCoAdmin);

                var coAdmin = new CollegeCoAdmin
                {
                    UserId = user.Id,
                    CollegeId = collegeId
                };
                await _db.CollegeCoAdmins.AddAsync(coAdmin);
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();

                // Send temp password via email
                await _emailSender.SendTemporaryPasswordAsync(email, tempPassword);

                return (true, null, tempPassword);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"فشل إنشاء الحساب: {ex.Message}", null);
            }
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> AddCoAdminRoleToExistingUserAsync(string userId, int collegeId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, "المستخدم غير موجود.");

            if (!await _db.Colleges.AnyAsync(c => c.Id == collegeId))
            {
                return (false, "الكلية غير موجودة.");
            }

            if (await _db.CollegeCoAdmins.AnyAsync(a => a.UserId == userId))
            {
                return (false, "المستخدم معين بالفعل كمدير لجنة لكلية.");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                if (!await _userManager.IsInRoleAsync(user, RoleConstants.CollegeCoAdmin))
                {
                    await _userManager.AddToRoleAsync(user, RoleConstants.CollegeCoAdmin);
                }

                var coAdmin = new CollegeCoAdmin
                {
                    UserId = user.Id,
                    CollegeId = collegeId
                };
                await _db.CollegeCoAdmins.AddAsync(coAdmin);
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> UpdateCoAdminSignatureAsync(int coAdminId, string signaturePath)
        {
            var coAdmin = await _db.CollegeCoAdmins.FindAsync(coAdminId);
            if (coAdmin == null) return (false, "مدير اللجنة غير موجود.");

            coAdmin.SignatureImagePath = signaturePath;
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<List<Judge>> GetAllJudgesAsync()
        {
            return await _db.Judges
                .Include(j => j.User)
                .Include(j => j.Evaluations)
                .OrderBy(j => j.User.FullName)
                .ToListAsync();
        }

        public async Task<List<JudgeInvitation>> GetAllJudgeInvitationsAsync()
        {
            return await _db.JudgeInvitations
                .Include(i => i.InvitedByUser)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<UserSummaryViewModel>> GetAssignableUsersForInvitationAsync()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var userIds = users.Select(u => u.Id).ToList();
            var userRoles = await _db.UserRoles
                .Where(ur => userIds.Contains(ur.UserId))
                .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, RoleName = r.Name })
                .ToListAsync();

            var roleLookup = userRoles
                .GroupBy(ur => ur.UserId)
                .ToDictionary(g => g.Key, g => g.First().RoleName);

            var list = new List<UserSummaryViewModel>();
            foreach (var u in users)
            {
                roleLookup.TryGetValue(u.Id, out var roleName);
                list.Add(new UserSummaryViewModel
                {
                    Email = u.Email ?? string.Empty,
                    FullName = string.IsNullOrWhiteSpace(u.FullName) ? (u.UserName ?? u.Email ?? "") : u.FullName,
                    RoleName = roleName ?? "مستخدم"
                });
            }
            return list;
        }

        public async Task<(bool Succeeded, string? ErrorMessage, string? RawToken)> InviteJudgeAsync(string email, string? specialty, string invitedByUserId)
        {
            var rawTokenBytes = new byte[32];
            RandomNumberGenerator.Fill(rawTokenBytes);
            var rawToken = Convert.ToHexString(rawTokenBytes).ToLowerInvariant();

            var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))).ToLowerInvariant();

            var invitation = new JudgeInvitation
            {
                Email = email,
                Specialty = specialty,
                InvitedByUserId = invitedByUserId,
                TokenHash = tokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                Status = InvitationStatusConstants.Pending
            };

            await _db.JudgeInvitations.AddAsync(invitation);
            await _db.SaveChangesAsync();

            return (true, null, rawToken);
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> SetJudgeActiveStatusAsync(int judgeId, bool isActive)
        {
            var judge = await _db.Judges.FindAsync(judgeId);
            if (judge == null) return (false, "المحكم غير موجود.");

            judge.IsActive = isActive;
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> RevokeInvitationAsync(int invitationId)
        {
            var inv = await _db.JudgeInvitations.FindAsync(invitationId);
            if (inv == null) return (false, "الدعوة غير موجودة.");

            inv.Status = InvitationStatusConstants.Revoked;
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<List<Research>> GetAllResearchesAsync()
        {
            return await _db.Researches
                .Include(r => r.College)
                .Include(r => r.Category)
                .Include(r => r.ResearcherResearchesBridges).ThenInclude(b => b.Researcher).ThenInclude(res => res.User)
                .Include(r => r.Evaluations).ThenInclude(e => e.Judge).ThenInclude(j => j.User)
                .OrderByDescending(r => r.SubmittedAt)
                .ToListAsync();
        }

        public async Task<Research?> GetResearchDetailsAsync(int researchId)
        {
            return await _db.Researches
                .Include(r => r.College)
                .Include(r => r.Category)
                .Include(r => r.ResearcherResearchesBridges).ThenInclude(b => b.Researcher).ThenInclude(res => res.User)
                .Include(r => r.ResearchAttachments)
                .Include(r => r.Evaluations).ThenInclude(e => e.Judge).ThenInclude(j => j.User)
                .Include(r => r.ResearchStatusHistories).ThenInclude(h => h.ChangedByUser)
                .FirstOrDefaultAsync(r => r.Id == researchId);
        }

        public async Task<List<Research>> SearchCertificatesArchiveAsync(string? serialNumber, string? researcherName, int? collegeId, DateTime? fromDate, DateTime? toDate)
        {
            var query = _db.Researches
                .Include(r => r.College)
                .Include(r => r.Category)
                .Include(r => r.ResearcherResearchesBridges).ThenInclude(b => b.Researcher).ThenInclude(res => res.User)
                .Where(r => r.WorkflowStatus == ResearchStatusConstants.CertificateIssued);

            if (!string.IsNullOrWhiteSpace(serialNumber))
            {
                query = query.Where(r => r.CertificateSerialNumber != null && r.CertificateSerialNumber.Contains(serialNumber));
            }

            if (collegeId.HasValue && collegeId.Value > 0)
            {
                query = query.Where(r => r.CollegeId == collegeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(researcherName))
            {
                query = query.Where(r => r.ResearcherResearchesBridges.Any(b => b.Researcher.User.FullName.Contains(researcherName)));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(r => r.CertificateIssuedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                var endOfDay = toDate.Value.Date.AddDays(1);
                query = query.Where(r => r.CertificateIssuedAt < endOfDay);
            }

            return await query.OrderByDescending(r => r.CertificateIssuedAt).ToListAsync();
        }

        public async Task<SystemStatisticsViewModel> GetSystemStatisticsAsync()
        {
            var researches = await _db.Researches
                .Include(r => r.College)
                .Include(r => r.Category)
                .ToListAsync();

            var total = researches.Count;
            var underScreening = researches.Count(r => r.WorkflowStatus == ResearchStatusConstants.UnderFormalScreening);
            var assigned = researches.Count(r => r.WorkflowStatus == ResearchStatusConstants.AssignedToJudges);
            var modifications = researches.Count(r => r.WorkflowStatus == ResearchStatusConstants.ModificationsRequired);
            var approved = researches.Count(r => r.WorkflowStatus == ResearchStatusConstants.Approved);
            var issued = researches.Count(r => r.WorkflowStatus == ResearchStatusConstants.CertificateIssued);

            var totalColleges = await _db.Colleges.CountAsync();
            var totalJudges = await _db.Judges.CountAsync(j => j.IsActive);
            var totalResearchers = await _db.Researchers.CountAsync();

            var byCollege = researches
                .GroupBy(r => r.College.Name)
                .ToDictionary(g => g.Key, g => g.Count());

            var byCategory = researches
                .GroupBy(r => r.Category.Name)
                .ToDictionary(g => g.Key, g => g.Count());

            // SLA calculation (screened within 48 hours)
            var histories = await _db.ResearchStatusHistories
                .Where(h => h.OldStatus == ResearchStatusConstants.UnderFormalScreening)
                .ToListAsync();

            var compliantCount = 0;
            var totalScreened = histories.Count;
            double totalHours = 0;

            foreach (var h in histories)
            {
                var research = researches.FirstOrDefault(r => r.Id == h.ResearchId);
                if (research != null)
                {
                    var hours = (h.ChangedAt - research.SubmittedAt).TotalHours;
                    totalHours += Math.Max(0, hours);
                    if (hours <= 48) compliantCount++;
                }
            }

            var avgScreeningHours = totalScreened > 0 ? totalHours / totalScreened : 0;
            var slaCompliance = totalScreened > 0 ? (compliantCount * 100.0 / totalScreened) : 100.0;

            // Judging days calculation
            var judgingHistories = await _db.ResearchStatusHistories
                .Where(h => h.OldStatus == ResearchStatusConstants.AssignedToJudges &&
                            (h.NewStatus == ResearchStatusConstants.Approved || h.NewStatus == ResearchStatusConstants.ModificationsRequired))
                .ToListAsync();

            var assignedHistories = await _db.ResearchStatusHistories
                .Where(h => h.NewStatus == ResearchStatusConstants.AssignedToJudges)
                .ToListAsync();

            double totalJudgingDays = 0;
            int closedRoundsCount = 0;
            foreach (var jh in judgingHistories)
            {
                var r = researches.FirstOrDefault(x => x.Id == jh.ResearchId);
                if (r != null)
                {
                    var assignTime = assignedHistories
                        .Where(a => a.ResearchId == jh.ResearchId && a.ChangedAt <= jh.ChangedAt)
                        .OrderByDescending(a => a.ChangedAt)
                        .FirstOrDefault()?.ChangedAt ?? r.SubmittedAt;

                    var days = (jh.ChangedAt - assignTime).TotalDays;
                    totalJudgingDays += Math.Max(0, days);
                    closedRoundsCount++;
                }
            }
            var avgJudgingDays = closedRoundsCount > 0 ? totalJudgingDays / closedRoundsCount : 0.0;

            return new SystemStatisticsViewModel
            {
                TotalResearches = total,
                UnderScreeningCount = underScreening,
                AssignedToJudgesCount = assigned,
                ModificationsRequiredCount = modifications,
                ApprovedCount = approved,
                CertificatesIssuedCount = issued,
                TotalColleges = totalColleges,
                TotalJudges = totalJudges,
                TotalResearchers = totalResearchers,
                AverageScreeningHours = Math.Round(avgScreeningHours, 1),
                AverageJudgingDays = Math.Round(avgJudgingDays, 1),
                SlaCompliancePercentage = Math.Round(slaCompliance, 1),
                ResearchesByCollege = byCollege,
                ResearchesByCategory = byCategory
            };
        }
    }
}
