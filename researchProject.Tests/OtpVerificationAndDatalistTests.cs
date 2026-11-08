using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using researchProject.Data;
using researchProject.Models.Entities;
using researchProject.Repositories.Implementations;
using researchProject.Services;
using Xunit;

namespace researchProject.Tests
{
    public class OtpVerificationAndDatalistTests
    {
        private ApplicationDbContext CreateInMemoryDb(string name)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(name)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ApplicationDbContext(options);
        }

        private (Mock<UserManager<ApplicationUser>> UserMgr, Mock<SignInManager<ApplicationUser>> SignInMgr, Mock<RoleManager<IdentityRole>> RoleMgr, Mock<IEmailSender> EmailSender) CreateMocks()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userMgr = new Mock<UserManager<ApplicationUser>>(
                userStore.Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<ApplicationUser>>().Object,
                new IUserValidator<ApplicationUser>[0],
                new IPasswordValidator<ApplicationUser>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<ApplicationUser>>>().Object);

            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            var signInMgr = new Mock<SignInManager<ApplicationUser>>(
                userMgr.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<ILogger<SignInManager<ApplicationUser>>>().Object,
                new Mock<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>().Object,
                new Mock<IUserConfirmation<ApplicationUser>>().Object);

            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            var roleMgr = new Mock<RoleManager<IdentityRole>>(
                roleStore.Object,
                new IRoleValidator<IdentityRole>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<ILogger<RoleManager<IdentityRole>>>().Object);

            var emailSender = new Mock<IEmailSender>();

            return (userMgr, signInMgr, roleMgr, emailSender);
        }

        private IConfiguration CreateTestConfig()
        {
            var dict = new Dictionary<string, string?>
            {
                { "Auth:AllowedEmailDomains:0", "univ.edu.eg" }
            };
            return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
        }

        [Fact]
        public async Task GenerateVerificationCode_Generates6DigitsAndSets15MinuteExpiration()
        {
            var db = CreateInMemoryDb(nameof(GenerateVerificationCode_Generates6DigitsAndSets15MinuteExpiration));
            var (userMgr, signInMgr, roleMgr, emailSender) = CreateMocks();
            var repo = new AccountRepository(userMgr.Object, signInMgr.Object, roleMgr.Object, db, CreateTestConfig(), emailSender.Object);

            var email = "researcher.test@example.com";
            var (ok, err, code, _) = await repo.GenerateVerificationCodeAsync(email, "EmailConfirmation");

            Assert.True(ok);
            Assert.NotNull(code);
            Assert.Equal(6, code!.Length);
            Assert.True(int.TryParse(code, out _), "OTP must be a numeric string");

            var saved = await db.EmailVerificationCodes.FirstOrDefaultAsync(c => c.Email == email);
            Assert.NotNull(saved);
            Assert.Equal(code, saved!.Code);
            Assert.False(saved.IsUsed);
            Assert.Equal(0, saved.AttemptsCount);

            // Verify strict 15-minute expiration
            var duration = saved.ExpiresAt - saved.CreatedAt;
            Assert.Equal(15, Math.Round(duration.TotalMinutes));
        }

        [Fact]
        public async Task ValidateVerificationCode_WithCorrectCode_SucceedsAndMarksIsUsedAndActivatesEmail()
        {
            var db = CreateInMemoryDb(nameof(ValidateVerificationCode_WithCorrectCode_SucceedsAndMarksIsUsedAndActivatesEmail));
            var (userMgr, signInMgr, roleMgr, emailSender) = CreateMocks();

            var testUser = new ApplicationUser
            {
                Id = "usr-1",
                Email = "dr.ahmed@test.edu",
                UserName = "dr.ahmed@test.edu",
                EmailConfirmed = false
            };

            userMgr.Setup(m => m.FindByEmailAsync(testUser.Email)).ReturnsAsync(testUser);
            userMgr.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

            var repo = new AccountRepository(userMgr.Object, signInMgr.Object, roleMgr.Object, db, CreateTestConfig(), emailSender.Object);

            var (ok, _, code, _) = await repo.GenerateVerificationCodeAsync(testUser.Email, "EmailConfirmation", testUser.Id);
            Assert.True(ok);

            var (validOk, validErr) = await repo.ValidateVerificationCodeAsync(testUser.Email, code!, "EmailConfirmation");
            Assert.True(validOk, validErr);

            var record = await db.EmailVerificationCodes.FirstOrDefaultAsync(c => c.Email == testUser.Email);
            Assert.NotNull(record);
            Assert.True(record!.IsUsed);
            Assert.NotNull(record.UsedAt);
            Assert.True(testUser.EmailConfirmed, "User email should be confirmed after valid OTP");
        }

        [Fact]
        public async Task ValidateVerificationCode_WithExpiredCode_FailsWith15MinuteMessage()
        {
            var db = CreateInMemoryDb(nameof(ValidateVerificationCode_WithExpiredCode_FailsWith15MinuteMessage));
            var (userMgr, signInMgr, roleMgr, emailSender) = CreateMocks();
            var repo = new AccountRepository(userMgr.Object, signInMgr.Object, roleMgr.Object, db, CreateTestConfig(), emailSender.Object);

            var email = "expired.otp@test.com";
            var (ok, _, code, _) = await repo.GenerateVerificationCodeAsync(email, "EmailConfirmation");
            Assert.True(ok);

            // Simulate expired code (> 15 minutes old)
            var record = await db.EmailVerificationCodes.FirstAsync(c => c.Email == email);
            record.CreatedAt = DateTime.UtcNow.AddMinutes(-20);
            record.ExpiresAt = DateTime.UtcNow.AddMinutes(-5);
            await db.SaveChangesAsync();

            var (validOk, validErr) = await repo.ValidateVerificationCodeAsync(email, code!, "EmailConfirmation");
            Assert.False(validOk);
            Assert.Contains("15 دقيقة", validErr);
        }

        [Fact]
        public async Task ValidateVerificationCode_WithWrongCode_IncrementsAttemptsAndBlocksAfter5()
        {
            var db = CreateInMemoryDb(nameof(ValidateVerificationCode_WithWrongCode_IncrementsAttemptsAndBlocksAfter5));
            var (userMgr, signInMgr, roleMgr, emailSender) = CreateMocks();
            var repo = new AccountRepository(userMgr.Object, signInMgr.Object, roleMgr.Object, db, CreateTestConfig(), emailSender.Object);

            var email = "bruteforce.test@test.com";
            var (ok, _, correctCode, _) = await repo.GenerateVerificationCodeAsync(email, "EmailConfirmation");
            Assert.True(ok);

            // 4 wrong attempts
            for (int i = 1; i <= 4; i++)
            {
                var (res, err) = await repo.ValidateVerificationCodeAsync(email, "000000", "EmailConfirmation");
                Assert.False(res);
                Assert.Contains("المحاولات المتبقية", err);
            }

            // 5th wrong attempt triggers lockout
            var (res5, err5) = await repo.ValidateVerificationCodeAsync(email, "000000", "EmailConfirmation");
            Assert.False(res5);
            Assert.Contains("تجاوز الحد الأقصى للمحاولات", err5);

            // Even entering correct code now fails because attempts exceeded
            var (res6, _) = await repo.ValidateVerificationCodeAsync(email, correctCode!, "EmailConfirmation");
            Assert.False(res6);
        }

        [Fact]
        public async Task ValidateVerificationCode_CannotBeReusedAfterSuccess()
        {
            var db = CreateInMemoryDb(nameof(ValidateVerificationCode_CannotBeReusedAfterSuccess));
            var (userMgr, signInMgr, roleMgr, emailSender) = CreateMocks();

            var user = new ApplicationUser { Id = "u2", Email = "reuse.test@test.com", EmailConfirmed = false };
            userMgr.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            userMgr.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

            var repo = new AccountRepository(userMgr.Object, signInMgr.Object, roleMgr.Object, db, CreateTestConfig(), emailSender.Object);

            var (_, _, code, _) = await repo.GenerateVerificationCodeAsync(user.Email, "EmailConfirmation", user.Id);

            // First validation succeeds
            var (firstOk, _) = await repo.ValidateVerificationCodeAsync(user.Email, code!, "EmailConfirmation");
            Assert.True(firstOk);

            // Second validation with same code fails
            var (secondOk, secondErr) = await repo.ValidateVerificationCodeAsync(user.Email, code!, "EmailConfirmation");
            Assert.False(secondOk);
            Assert.Contains("لا يوجد رمز تحقق نشط", secondErr);
        }

        [Fact]
        public async Task ResendVerificationCode_EnforcesCooldownRateLimiting()
        {
            var db = CreateInMemoryDb(nameof(ResendVerificationCode_EnforcesCooldownRateLimiting));
            var (userMgr, signInMgr, roleMgr, emailSender) = CreateMocks();

            var user = new ApplicationUser { Id = "u3", Email = "cooldown.test@test.com", EmailConfirmed = false };
            userMgr.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);

            var repo = new AccountRepository(userMgr.Object, signInMgr.Object, roleMgr.Object, db, CreateTestConfig(), emailSender.Object);

            // 1. Initial generation
            await repo.GenerateVerificationCodeAsync(user.Email, "EmailConfirmation", user.Id);

            // 2. Immediate resend should be rate-limited (under 60s)
            var (immediateOk, immediateErr, _) = await repo.ResendVerificationCodeAsync(user.Email, "EmailConfirmation");
            Assert.False(immediateOk);
            Assert.Contains("ثانية", immediateErr);

            // 3. Fast-forward last code createdAt to simulate 65 seconds elapsed
            var record = await db.EmailVerificationCodes.FirstAsync(c => c.Email == user.Email);
            record.CreatedAt = DateTime.UtcNow.AddSeconds(-65);
            await db.SaveChangesAsync();

            // 4. Resend now succeeds and creates fresh code
            var (resendOk, _, newCode) = await repo.ResendVerificationCodeAsync(user.Email, "EmailConfirmation");
            Assert.True(resendOk);
            Assert.NotNull(newCode);
            Assert.Equal(6, newCode!.Length);

            // Previous code should be marked as used/invalidated
            Assert.True(record.IsUsed, "Old code must be invalidated upon generating a new code");
        }

        [Fact]
        public async Task ResendVerificationCode_FailsIfUserNotFound()
        {
            var db = CreateInMemoryDb(nameof(ResendVerificationCode_FailsIfUserNotFound));
            var (userMgr, signInMgr, roleMgr, emailSender) = CreateMocks();
            userMgr.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

            var repo = new AccountRepository(userMgr.Object, signInMgr.Object, roleMgr.Object, db, CreateTestConfig(), emailSender.Object);

            var (ok, err, _) = await repo.ResendVerificationCodeAsync("unknown@test.com", "EmailConfirmation");
            Assert.False(ok);
            Assert.Contains("غير مسجل", err);
        }

        [Fact]
        public async Task ResendVerificationCode_FailsIfEmailAlreadyConfirmed()
        {
            var db = CreateInMemoryDb(nameof(ResendVerificationCode_FailsIfEmailAlreadyConfirmed));
            var (userMgr, signInMgr, roleMgr, emailSender) = CreateMocks();

            var user = new ApplicationUser { Id = "u4", Email = "active@test.com", EmailConfirmed = true };
            userMgr.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);

            var repo = new AccountRepository(userMgr.Object, signInMgr.Object, roleMgr.Object, db, CreateTestConfig(), emailSender.Object);

            var (ok, err, _) = await repo.ResendVerificationCodeAsync(user.Email, "EmailConfirmation");
            Assert.False(ok);
            Assert.Contains("مفعل بالفعل", err);
        }
    }
}
