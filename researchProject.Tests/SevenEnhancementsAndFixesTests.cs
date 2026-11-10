using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using researchProject.Controllers;
using researchProject.Data;
using researchProject.Models.DTOs;
using researchProject.Models.Entities;
using researchProject.Repositories.Implementations;
using researchProject.Repositories.Interfaces;
using researchProject.Services;
using Xunit;

namespace researchProject.Tests
{
    public class SevenEnhancementsAndFixesTests
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
                { "Auth:AllowedEmailDomains:0", "univ.edu.eg" }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();
        }

        [Fact]
        public async Task SignUpResearcher_RejectsDuplicateNationalId()
        {
            // Arrange
            var db = CreateInMemoryDb("NationalId_Duplicate_Rejection");
            var config = CreateTestConfig();
            var emailMock = new Mock<IEmailSender>();

            // Existing researcher in DB with NationalId "30211122500859"
            var existingUser = new ApplicationUser
            {
                Id = "existing-user-1",
                Email = "existing@example.com",
                UserName = "existing@example.com",
                FullName = "باحث قديم",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(existingUser);
            db.Researchers.Add(new Researcher
            {
                UserId = existingUser.Id,
                NationalId = "30211122500859",
                CollegeId = 1,
                AcademicDegree = "دكتوراه",
                ResearcherType = "عضو هيئة تدريس"
            });
            await db.SaveChangesAsync();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
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
                emailMock.Object);

            var newDto = new ResearcherSignUpDto
            {
                FullName = "باحث جديد",
                Email = "new@example.com",
                Password = "Password123!",
                NationalId = "30211122500859", // duplicate!
                CollegeId = 1,
                AcademicDegree = "ماجستير",
                ResearcherType = "باحث دراسات عليا"
            };

            // Act
            var (succ, err, _, _) = await repo.SignUpResearcherAsync(newDto);

            // Assert
            Assert.False(succ);
            Assert.Contains("الرقم القومي المدخل مسجل بالفعل", err);
        }

        [Fact]
        public async Task CleanupUnconfirmedAccounts_DeletesAccountsOlderThan24Hours()
        {
            // Arrange
            var db = CreateInMemoryDb("Cleanup_Unconfirmed_Accounts");
            var config = CreateTestConfig();
            var emailMock = new Mock<IEmailSender>();

            // 1. Account created 30 hours ago, unconfirmed -> MUST BE DELETED
            var oldUnconfirmed = new ApplicationUser
            {
                Id = "old-unconf",
                Email = "old.unconf@example.com",
                UserName = "old.unconf@example.com",
                FullName = "مستخدم قديم غير مؤكد",
                EmailConfirmed = false,
                CreatedAt = DateTime.UtcNow.AddHours(-30)
            };
            db.Users.Add(oldUnconfirmed);
            db.Researchers.Add(new Researcher
            {
                UserId = oldUnconfirmed.Id,
                NationalId = "11111111111111",
                CollegeId = 1,
                AcademicDegree = "ماجستير",
                ResearcherType = "باحث دراسات عليا"
            });
            db.EmailVerificationCodes.Add(new EmailVerificationCode
            {
                UserId = oldUnconfirmed.Id,
                Code = "123456",
                CreatedAt = DateTime.UtcNow.AddHours(-30),
                ExpiresAt = DateTime.UtcNow.AddHours(-29)
            });

            // 2. Account created 2 hours ago, unconfirmed -> MUST NOT BE DELETED
            var recentUnconfirmed = new ApplicationUser
            {
                Id = "recent-unconf",
                Email = "recent.unconf@example.com",
                UserName = "recent.unconf@example.com",
                FullName = "مستخدم حديث غير مؤكد",
                EmailConfirmed = false,
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            };
            db.Users.Add(recentUnconfirmed);

            // 3. Account created 40 hours ago, confirmed -> MUST NOT BE DELETED
            var oldConfirmed = new ApplicationUser
            {
                Id = "old-conf",
                Email = "old.conf@example.com",
                UserName = "old.conf@example.com",
                FullName = "مستخدم مؤكد",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow.AddHours(-40)
            };
            db.Users.Add(oldConfirmed);

            await db.SaveChangesAsync();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            userManagerMock.Setup(u => u.Users).Returns(db.Users);
            userManagerMock.Setup(u => u.DeleteAsync(It.IsAny<ApplicationUser>()))
                .Returns<ApplicationUser>(async u =>
                {
                    db.Users.Remove(u);
                    await db.SaveChangesAsync();
                    return IdentityResult.Success;
                });

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
                emailMock.Object);

            // Act
            int deletedCount = await repo.CleanupUnconfirmedAccountsAsync();

            // Assert
            Assert.Equal(1, deletedCount);
            Assert.Null(await db.Users.FindAsync("old-unconf"));
            Assert.Null(await db.Researchers.FirstOrDefaultAsync(r => r.UserId == "old-unconf"));
            Assert.Empty(await db.EmailVerificationCodes.Where(c => c.UserId == "old-unconf").ToListAsync());
            Assert.NotNull(await db.Users.FindAsync("recent-unconf"));
            Assert.NotNull(await db.Users.FindAsync("old-conf"));
        }

        [Fact]
        public async Task ProfileManagement_GetAndUpdateProfile_WorksCorrectly()
        {
            // Arrange
            var db = CreateInMemoryDb("Profile_Management_Test");
            var config = CreateTestConfig();
            var emailMock = new Mock<IEmailSender>();

            var user = new ApplicationUser
            {
                Id = "user-profile-id",
                Email = "user@example.com",
                UserName = "user@example.com",
                FullName = "الاسم القديم",
                PhoneNumber = "01000000000",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            userManagerMock.Setup(u => u.FindByIdAsync("user-profile-id")).ReturnsAsync(user);
            userManagerMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Researcher" });
            userManagerMock.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

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
                emailMock.Object);

            // Act 1: Get Profile View Model
            var vm = await repo.GetProfileViewModelAsync("user-profile-id");
            Assert.NotNull(vm);
            Assert.Equal("user@example.com", vm.Email);
            Assert.Equal("الاسم القديم", vm.FullName);
            Assert.Equal("01000000000", vm.PhoneNumber);

            // Act 2: Update Profile
            var updateDto = new UpdateProfileDto
            {
                FullName = "الاسم الجديد المحدث",
                PhoneNumber = "01123456789"
            };
            var (succ, err) = await repo.UpdateProfileAsync("user-profile-id", updateDto);

            // Assert
            Assert.True(succ, err);
            Assert.Equal("الاسم الجديد المحدث", user.FullName);
            Assert.Equal("01123456789", user.PhoneNumber);
        }

        [Fact]
        public async Task ProfileManagement_ResearcherAndJudge_CanUpdateAcademicDegreeAndResearcherType()
        {
            // Arrange
            var db = CreateInMemoryDb("Profile_Researcher_Judge_Update_Test");
            var config = CreateTestConfig();
            var emailMock = new Mock<IEmailSender>();

            var college = new College { Name = "كلية الهندسة", Code = "ENG" };
            db.Colleges.Add(college);

            var resUser = new ApplicationUser
            {
                Id = "res-user-id",
                Email = "researcher@univ.edu.eg",
                FullName = "د. الباحث التجريبي",
                PhoneNumber = "01011112222",
                EmailConfirmed = true
            };
            db.Users.Add(resUser);

            var researcher = new Researcher
            {
                UserId = resUser.Id,
                CollegeId = college.Id,
                ResearcherType = "طالب دراسات عليا",
                AcademicDegree = "بكالوريوس",
                Department = "هندسة حيوية",
                NationalId = "29401011234567"
            };
            db.Researchers.Add(researcher);

            var judgeUser = new ApplicationUser
            {
                Id = "judge-user-id",
                Email = "judge@univ.edu.eg",
                FullName = "د. المحكم المعتمد",
                PhoneNumber = "01033334444",
                EmailConfirmed = true
            };
            db.Users.Add(judgeUser);

            var judge = new Judge
            {
                UserId = judgeUser.Id,
                CollegeId = college.Id,
                Specialty = "هندسة نظم حيوية",
                AcademicTitle = "مدرس",
                ResearcherType = "باحث"
            };
            db.Judges.Add(judge);
            await db.SaveChangesAsync();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            userManagerMock.Setup(u => u.FindByIdAsync("res-user-id")).ReturnsAsync(resUser);
            userManagerMock.Setup(u => u.FindByIdAsync("judge-user-id")).ReturnsAsync(judgeUser);
            userManagerMock.Setup(u => u.GetRolesAsync(resUser)).ReturnsAsync(new List<string> { "Researcher" });
            userManagerMock.Setup(u => u.GetRolesAsync(judgeUser)).ReturnsAsync(new List<string> { "Judge" });
            userManagerMock.Setup(u => u.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

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
                emailMock.Object);

            // Act 1: Researcher updates degree & researcher type
            var resDto = new UpdateProfileDto
            {
                FullName = "د. الباحث المحدث",
                PhoneNumber = "01099998888",
                AcademicDegree = "دكتوراه",
                ResearcherType = "عضو هيئة تدريس"
            };
            var (resSucc, resErr) = await repo.UpdateProfileAsync("res-user-id", resDto);
            Assert.True(resSucc, resErr);

            // Verify Researcher entity updated
            var updatedResearcher = await db.Researchers.FirstAsync(r => r.UserId == "res-user-id");
            Assert.Equal("دكتوراه", updatedResearcher.AcademicDegree);
            Assert.Equal("عضو هيئة تدريس", updatedResearcher.ResearcherType);

            // Verify ViewModel reflects updated researcher data
            var resVm = await repo.GetProfileViewModelAsync("res-user-id");
            Assert.NotNull(resVm);
            Assert.Equal("دكتوراه", resVm.AcademicDegree);
            Assert.Equal("عضو هيئة تدريس", resVm.ResearcherType);

            // Act 2: Judge updates degree & researcher type
            var judgeDto = new UpdateProfileDto
            {
                FullName = "أ.د. المحكم المحدث",
                PhoneNumber = "01055556666",
                AcademicDegree = "أستاذ دكتور",
                ResearcherType = "مستشار خبير"
            };
            var (judgeSucc, judgeErr) = await repo.UpdateProfileAsync("judge-user-id", judgeDto);
            Assert.True(judgeSucc, judgeErr);

            // Verify Judge entity updated
            var updatedJudge = await db.Judges.FirstAsync(j => j.UserId == "judge-user-id");
            Assert.Equal("أستاذ دكتور", updatedJudge.AcademicTitle);
            Assert.Equal("مستشار خبير", updatedJudge.ResearcherType);

            // Verify ViewModel reflects updated judge data
            var judgeVm = await repo.GetProfileViewModelAsync("judge-user-id");
            Assert.NotNull(judgeVm);
            Assert.Equal("أستاذ دكتور", judgeVm.AcademicDegree);
            Assert.Equal("مستشار خبير", judgeVm.ResearcherType);
        }

        [Fact]
        public async Task ProfileManagement_ChangePassword_EnforcesIdentityResult()
        {
            // Arrange
            var db = CreateInMemoryDb("ChangePassword_Test");
            var config = CreateTestConfig();
            var emailMock = new Mock<IEmailSender>();

            var user = new ApplicationUser
            {
                Id = "pw-user-id",
                Email = "pw@example.com",
                UserName = "pw@example.com",
                FullName = "باحث أمان"
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            userManagerMock.Setup(u => u.FindByIdAsync("pw-user-id")).ReturnsAsync(user);
            userManagerMock.Setup(u => u.ChangePasswordAsync(user, "OldPass123!", "NewPass456!"))
                .ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(u => u.ChangePasswordAsync(user, "WrongPass!", "NewPass456!"))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "كلمة المرور الحالية غير صحيحة." }));

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
                emailMock.Object);

            // Act - Valid
            var (validSucc, _) = await repo.ChangePasswordAsync("pw-user-id", new ChangePasswordDto
            {
                CurrentPassword = "OldPass123!",
                NewPassword = "NewPass456!",
                ConfirmPassword = "NewPass456!"
            });

            // Act - Invalid
            var (failSucc, failErr) = await repo.ChangePasswordAsync("pw-user-id", new ChangePasswordDto
            {
                CurrentPassword = "WrongPass!",
                NewPassword = "NewPass456!",
                ConfirmPassword = "NewPass456!"
            });

            // Assert
            Assert.True(validSucc);
            Assert.False(failSucc);
            Assert.Contains("كلمة المرور الحالية غير صحيحة", failErr);
        }

        [Fact]
        public async Task ResearcherController_CreateResearch_ReturnsClearErrorWhenCategoryIdZero()
        {
            // Arrange
            var repoMock = new Mock<IResearcherRepository>();
            var controller = new ResearcherController(repoMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-researcher-user-id")
            }, "TestAuth"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var invalidDto = new ResearchCreateDto
            {
                Title = "بحث جديد بدون تصنيف محدد",
                AcademicSpecialty = "صيدلة",
                CollegeId = 1,
                CategoryId = 0 // Invalid CategoryId (failed autocomplete or typing)
            };

            // Act
            var result = await controller.CreateResearch(invalidDto) as JsonResult;

            // Assert
            Assert.NotNull(result);
            var value = result.Value;
            var succProp = value?.GetType().GetProperty("success")?.GetValue(value) as bool?;
            var msgProp = value?.GetType().GetProperty("message")?.GetValue(value) as string;

            Assert.False(succProp);
            Assert.Contains("تصنيف البحث", msgProp);
        }
    }
}
