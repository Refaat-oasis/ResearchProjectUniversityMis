using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using researchProject.Data;
using researchProject.Models.Constants;
using researchProject.Models.Entities;
using researchProject.Models.Enums;
using researchProject.Repositories.Implementations;
using researchProject.Services;
using Xunit;

namespace researchProject.Tests
{
    public class NewRequirementsAndEnhancementsTests
    {
        private ApplicationDbContext CreateInMemoryDb(string name)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(name)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ApplicationDbContext(options);
        }

        private IConfiguration CreateTestConfig()
        {
            var configData = new Dictionary<string, string?>
            {
                { "Auth:AllowedEmailDomains:0", "univ.edu.eg" },
                { "Screening:SlaHours", "48" },
                { "Evaluation:MinJudgesPerResearch", "1" },
                { "FileStorage:RootPath", "App_Data/UploadedFiles" }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();
        }

        [Fact]
        public void Enums_ToArabicString_MapsCorrectlyToConstants()
        {
            // ResearchStatus
            Assert.Equal(ResearchStatusConstants.UnderFormalScreening, ResearchStatus.UnderFormalScreening.ToArabicString());
            Assert.Equal(ResearchStatusConstants.AssignedToJudges, ResearchStatus.AssignedToJudges.ToArabicString());
            Assert.Equal(ResearchStatusConstants.ModificationsRequired, ResearchStatus.ModificationsRequired.ToArabicString());
            Assert.Equal(ResearchStatusConstants.Approved, ResearchStatus.Approved.ToArabicString());
            Assert.Equal(ResearchStatusConstants.CertificateIssued, ResearchStatus.CertificateIssued.ToArabicString());

            // UserRole
            Assert.Equal("مدير النظام العام", UserRole.SuperAdmin.ToArabicString());
            Assert.Equal("مدير اللجنة الفرعية بالكلية", UserRole.CollegeCoAdmin.ToArabicString());
            Assert.Equal("محكم علمي", UserRole.Judge.ToArabicString());
            Assert.Equal("باحث", UserRole.Researcher.ToArabicString());

            // EvaluationDecision
            Assert.Equal(EvaluationDecisionConstants.Approved, EvaluationDecision.Approved.ToArabicString());
            Assert.Equal(EvaluationDecisionConstants.ModificationsRequired, EvaluationDecision.ModificationsRequired.ToArabicString());
            Assert.Equal(EvaluationDecisionConstants.ClosedDueToOtherJudge, EvaluationDecision.ClosedDueToOtherJudge.ToArabicString());

            // ResearcherType
            Assert.Equal(ResearcherTypeConstants.FacultyMember, ResearcherType.FacultyMember.ToArabicString());
            Assert.Equal(ResearcherTypeConstants.PostgraduateStudent, ResearcherType.PostgraduateStudent.ToArabicString());

            // EthicsRating
            Assert.Equal(EthicsRatingConstants.Appropriate, EthicsRating.Appropriate.ToArabicString());
            Assert.Equal(EthicsRatingConstants.NeedsModification, EthicsRating.NeedsModification.ToArabicString());
            Assert.Equal(EthicsRatingConstants.Inappropriate, EthicsRating.Inappropriate.ToArabicString());

            // Reverse Parsing tests
            Assert.Equal(ResearchStatus.UnderFormalScreening, "قيد المراجعة الشكلية".ToResearchStatus());
            Assert.Equal(ResearchStatus.Approved, "Approved".ToResearchStatus());
            Assert.Equal(UserRole.SuperAdmin, "SuperAdmin".ToUserRole());
            Assert.Equal(UserRole.Judge, "محكم علمي".ToUserRole());
            Assert.Equal(EvaluationDecision.ClosedDueToOtherJudge, "مغلق لاكتمال التحكيم".ToEvaluationDecision());
            Assert.Equal(EthicsRating.Appropriate, "مناسب".ToEthicsRating());
            Assert.Equal(ResearcherType.FacultyMember, "عضو هيئة تدريس".ToResearcherType());
            Assert.Equal(InvitationStatus.Pending, "معلق".ToInvitationStatus());
            Assert.Equal(AttachmentType.InformedConsent, "الموافقة المستنيرة".ToAttachmentType());
        }

        [Fact]
        public async Task DepartmentManagement_SuperAdminCanAddEditAndDeleteDepartments()
        {
            var db = CreateInMemoryDb("SuperAdmin_DepartmentManagement");
            var emailMock = new Mock<IEmailSender>();
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            var roleManagerMock = new Mock<RoleManager<IdentityRole>>(roleStoreMock.Object, null!, null!, null!, null!);

            var repo = new SuperAdminRepository(db, userManagerMock.Object, roleManagerMock.Object, emailMock.Object);

            var college = new College { Name = "كلية الطب البشري", Code = "MED_TEST" };
            db.Colleges.Add(college);
            await db.SaveChangesAsync();

            // 1. Add Department
            var (addSucc, addErr) = await repo.SaveDepartmentAsync(null, "قسم الجراحة العامة", college.Id);
            Assert.True(addSucc, addErr);

            var departments = await repo.GetDepartmentsByCollegeAsync(college.Id);
            Assert.Single(departments);
            Assert.Equal("قسم الجراحة العامة", departments[0].Name);
            var depId = departments[0].Id;

            // 2. Edit Department
            var (editSucc, editErr) = await repo.SaveDepartmentAsync(depId, "قسم الجراحة العامة والمناظير", college.Id);
            Assert.True(editSucc, editErr);

            var updatedDep = await db.Departments.FindAsync(depId);
            Assert.Equal("قسم الجراحة العامة والمناظير", updatedDep?.Name);

            // 3. Delete Department
            var (delSucc, delErr) = await repo.DeleteDepartmentAsync(depId);
            Assert.True(delSucc, delErr);

            var emptyList = await repo.GetDepartmentsByCollegeAsync(college.Id);
            Assert.Empty(emptyList);
        }

        [Fact]
        public async Task AccountRepository_PasswordResetFlow_GeneratesTokenAndResetsSuccessfully()
        {
            var db = CreateInMemoryDb("Account_PasswordReset");
            var config = CreateTestConfig();
            var emailSenderMock = new Mock<IEmailSender>();

            var user = new ApplicationUser
            {
                Id = "user-reset-pwd",
                Email = "resetuser@example.com",
                UserName = "resetuser@example.com",
                FullName = "مستخدم تجريبي"
            };

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            userManagerMock.Setup(u => u.FindByEmailAsync("resetuser@example.com")).ReturnsAsync(user);
            userManagerMock.Setup(u => u.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("valid-reset-token-999");
            userManagerMock.Setup(u => u.ResetPasswordAsync(user, "valid-reset-token-999", "NewPassword123!")).ReturnsAsync(IdentityResult.Success);

            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            var roleManagerMock = new Mock<RoleManager<IdentityRole>>(roleStoreMock.Object, null!, null!, null!, null!);
            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(userManagerMock.Object, contextAccessorMock.Object, claimsFactoryMock.Object, null!, null!, null!, null!);

            var repo = new AccountRepository(
                userManagerMock.Object,
                signInManagerMock.Object,
                roleManagerMock.Object,
                db,
                config,
                emailSenderMock.Object);

            // 1. Generate Token
            var (genSucc, genErr, token, userId) = await repo.GeneratePasswordResetTokenAsync("resetuser@example.com");
            Assert.True(genSucc, genErr);
            Assert.NotNull(token);
            Assert.Equal(6, token!.Length);
            Assert.True(int.TryParse(token, out _), "Reset token must be a 6-digit numeric OTP");
            Assert.Equal(user.Id, userId);

            // 2. Reset with Token
            var (resetSucc, resetErr) = await repo.ResetPasswordWithTokenAsync("resetuser@example.com", token, "NewPassword123!");
            Assert.True(resetSucc, resetErr);
        }

        [Fact]
        public async Task AccountRepository_ConfirmEmailWithTokenOrCode_ValidatesSuccessfully()
        {
            var db = CreateInMemoryDb("Account_ConfirmEmailToken");
            var config = CreateTestConfig();
            var emailSenderMock = new Mock<IEmailSender>();

            var user = new ApplicationUser
            {
                Id = "user-confirm-email",
                Email = "confirm@example.com",
                UserName = "confirm@example.com",
                FullName = "مستخدم التأكيد",
                EmailConfirmed = false
            };

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            userManagerMock.Setup(u => u.FindByEmailAsync("confirm@example.com")).ReturnsAsync(user);
            userManagerMock.Setup(u => u.ConfirmEmailAsync(user, "valid-confirm-token")).ReturnsAsync(IdentityResult.Success);

            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            var roleManagerMock = new Mock<RoleManager<IdentityRole>>(roleStoreMock.Object, null!, null!, null!, null!);
            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(userManagerMock.Object, contextAccessorMock.Object, claimsFactoryMock.Object, null!, null!, null!, null!);

            var repo = new AccountRepository(
                userManagerMock.Object,
                signInManagerMock.Object,
                roleManagerMock.Object,
                db,
                config,
                emailSenderMock.Object);

            var (succ, err) = await repo.ConfirmEmailWithTokenOrCodeAsync("confirm@example.com", "valid-confirm-token");
            Assert.True(succ, err);
        }

        [Fact]
        public async Task AssignableUsers_ReturnsSystemUsersForDatalistInvitation()
        {
            var db = CreateInMemoryDb("AssignableUsers_Datalist");
            var emailMock = new Mock<IEmailSender>();
            var certMock = new Mock<ICertificateService>();
            var loggerMock = new Mock<ILogger<CollegeCoAdminRepository>>();

            var user1 = new ApplicationUser { Id = "u1", FullName = "د. أحمد خليل", Email = "ahmed@example.com" };
            var user2 = new ApplicationUser { Id = "u2", FullName = "د. سارة فؤاد", Email = "sara@example.com" };
            db.Users.AddRange(user1, user2);
            await db.SaveChangesAsync();

            var role = new IdentityRole { Id = "r1", Name = "Judge", NormalizedName = "JUDGE" };
            db.Roles.Add(role);
            db.UserRoles.Add(new IdentityUserRole<string> { UserId = "u1", RoleId = "r1" });
            await db.SaveChangesAsync();

            var repo = new CollegeCoAdminRepository(db, certMock.Object, emailMock.Object, CreateTestConfig(), loggerMock.Object);

            var users = await repo.GetAssignableUsersForInvitationAsync();
            Assert.Equal(2, users.Count);
            Assert.Contains(users, u => u.Email == "ahmed@example.com");
            Assert.Contains(users, u => u.Email == "sara@example.com");
        }

        [Fact]
        public async Task AccountRepository_ConfirmEmail_WithDirectEmail_ResolvesAndConfirms()
        {
            var db = CreateInMemoryDb("Account_ConfirmWithDirectEmail");
            var config = CreateTestConfig();
            var emailSenderMock = new Mock<IEmailSender>();

            var user = new ApplicationUser
            {
                Id = "guid-1234-abcd",
                Email = "doctor@gmail.com",
                UserName = "doctor@gmail.com",
                FullName = "د. محمد السيد",
                EmailConfirmed = false
            };

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            userManagerMock.Setup(u => u.FindByEmailAsync("doctor@gmail.com")).ReturnsAsync(user);
            userManagerMock.Setup(u => u.ConfirmEmailAsync(user, "valid-token-xyz")).ReturnsAsync(IdentityResult.Success);

            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            var roleManagerMock = new Mock<RoleManager<IdentityRole>>(roleStoreMock.Object, null!, null!, null!, null!);
            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(userManagerMock.Object, contextAccessorMock.Object, claimsFactoryMock.Object, null!, null!, null!, null!);

            var repo = new AccountRepository(
                userManagerMock.Object,
                signInManagerMock.Object,
                roleManagerMock.Object,
                db,
                config,
                emailSenderMock.Object);

            var (succ, err) = await repo.ConfirmEmailWithTokenOrCodeAsync("doctor@gmail.com", "valid-token-xyz");
            Assert.True(succ, err);
        }
    }
}
