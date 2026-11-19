using System;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using researchProject.Controllers;
using researchProject.Data;
using researchProject.Filters;
using researchProject.Models;
using researchProject.Models.Constants;
using researchProject.Models.Entities;
using researchProject.Repositories.Implementations;
using researchProject.Services;
using researchProject.Services.Implementations;
using researchProject.Services.Interfaces;
using Xunit;

namespace researchProject.Tests
{
    public class HangfireAuditAndErrorHandlingTests
    {
        private ApplicationDbContext CreateInMemoryDb(string name)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(name)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ApplicationDbContext(options);
        }

        private Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        [Fact]
        public async Task HangfireEmailDispatcher_SecondFailure_RollsBackJudgeInvitation()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var db = CreateInMemoryDb(dbName);

            var invitation = new JudgeInvitation
            {
                Id = 55,
                Email = "failed.judge@uni.edu",
                Specialty = "علوم حاسب",
                InvitedByUserId = "admin-user-1",
                Status = InvitationStatusConstants.Pending,
                TokenHash = "testhash55"
            };
            db.JudgeInvitations.Add(invitation);
            await db.SaveChangesAsync();

            var backgroundJobsMock = new Mock<IBackgroundJobClient>();
            var userManagerMock = CreateUserManagerMock();
            var emailSenderMock = new Mock<IEmailSender>();
            var notificationServiceMock = new Mock<INotificationService>();
            var loggerMock = new Mock<ILogger<HangfireEmailDispatcher>>();

            // Setup email sender to fail
            emailSenderMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("SMTP connection refused"));

            var services = new ServiceCollection();
            services.AddScoped(_ => db);
            services.AddScoped(_ => userManagerMock.Object);
            services.AddScoped(_ => emailSenderMock.Object);
            services.AddScoped(_ => notificationServiceMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            var scopeMock = new Mock<IServiceScope>();
            scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProvider);
            scopeFactoryMock.Setup(s => s.CreateScope()).Returns(scopeMock.Object);

            var dispatcher = new HangfireEmailDispatcher(backgroundJobsMock.Object, scopeFactoryMock.Object, loggerMock.Object);

            // Act - Execute attempt #2 (Final attempt)
            await dispatcher.SendEmailWithRollbackAsync(
                "failed.judge@uni.edu",
                "Test Subject",
                "<p>Test</p>",
                EmailJobType.JudgeInvitation,
                "55",
                "admin-user-1",
                attemptNumber: 2);

            // Assert - The invitation MUST be deleted from the database
            var deletedInv = await db.JudgeInvitations.FirstOrDefaultAsync(i => i.Id == 55);
            Assert.Null(deletedInv);

            // Notification must be dispatched to the initiator
            notificationServiceMock.Verify(n => n.SendNotificationAsync(
                "admin-user-1",
                It.Is<string>(t => t.Contains("فشل")),
                It.Is<string>(m => m.Contains("failed.judge@uni.edu")),
                It.IsAny<string>(),
                "Error",
                It.IsAny<string>()), Times.Once);

            // Failure Audit Log must be created
            var audit = await db.AuditLogs.FirstOrDefaultAsync(a => a.ActionName == "Rollback_JudgeInvitation");
            Assert.NotNull(audit);
            Assert.False(audit.IsSuccess);
            Assert.Contains("failed.judge@uni.edu", audit.Description);
        }

        [Fact]
        public async Task HangfireEmailDispatcher_SecondFailure_RollsBackCoAdminAccount()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var db = CreateInMemoryDb(dbName);

            var coAdmin = new CollegeCoAdmin
            {
                UserId = "coadmin-user-99",
                CollegeId = 1
            };
            db.CollegeCoAdmins.Add(coAdmin);
            await db.SaveChangesAsync();

            var testUser = new ApplicationUser
            {
                Id = "coadmin-user-99",
                Email = "coadmin.failed@uni.edu",
                FullName = "مشرف تجريبي"
            };

            var backgroundJobsMock = new Mock<IBackgroundJobClient>();
            var userManagerMock = CreateUserManagerMock();
            userManagerMock.Setup(m => m.FindByIdAsync("coadmin-user-99")).ReturnsAsync(testUser);
            userManagerMock.Setup(m => m.DeleteAsync(testUser)).ReturnsAsync(IdentityResult.Success);

            var emailSenderMock = new Mock<IEmailSender>();
            var notificationServiceMock = new Mock<INotificationService>();
            var loggerMock = new Mock<ILogger<HangfireEmailDispatcher>>();

            emailSenderMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("SMTP timeout"));

            var services = new ServiceCollection();
            services.AddScoped(_ => db);
            services.AddScoped(_ => userManagerMock.Object);
            services.AddScoped(_ => emailSenderMock.Object);
            services.AddScoped(_ => notificationServiceMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            var scopeMock = new Mock<IServiceScope>();
            scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProvider);
            scopeFactoryMock.Setup(s => s.CreateScope()).Returns(scopeMock.Object);

            var dispatcher = new HangfireEmailDispatcher(backgroundJobsMock.Object, scopeFactoryMock.Object, loggerMock.Object);

            // Act - Execute attempt #2 (Final attempt)
            await dispatcher.SendEmailWithRollbackAsync(
                "coadmin.failed@uni.edu",
                "Credentials",
                "<p>Test</p>",
                EmailJobType.CoAdminCredentials,
                "coadmin-user-99",
                "super-admin-id",
                attemptNumber: 2);

            // Assert - CoAdmin record must be deleted
            var deletedCoAdmin = await db.CollegeCoAdmins.FirstOrDefaultAsync(c => c.UserId == "coadmin-user-99");
            Assert.Null(deletedCoAdmin);

            // User must be deleted via UserManager
            userManagerMock.Verify(m => m.DeleteAsync(testUser), Times.Once);

            // Rollback Audit Log must exist
            var audit = await db.AuditLogs.FirstOrDefaultAsync(a => a.ActionName == "Rollback_CoAdminCredentials");
            Assert.NotNull(audit);
            Assert.False(audit.IsSuccess);
        }

        [Fact]
        public async Task FileStorageService_SaveFileAsync_SavesInWwwRootAndReturnsRelativePath()
        {
            // Arrange
            var tempFolder = Path.Combine(Path.GetTempPath(), "ResearchProjectTest_" + Guid.NewGuid().ToString("N"));
            var wwwRoot = Path.Combine(tempFolder, "wwwroot");
            Directory.CreateDirectory(wwwRoot);

            try
            {
                var envMock = new Mock<IWebHostEnvironment>();
                envMock.Setup(e => e.WebRootPath).Returns(wwwRoot);
                envMock.Setup(e => e.ContentRootPath).Returns(tempFolder);

                var inMemorySettings = new System.Collections.Generic.Dictionary<string, string?>
                {
                    {"FileStorage:RootPath", ""}
                };
                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(inMemorySettings)
                    .Build();

                var storageService = new FileStorageService(envMock.Object, configuration);

                var fileMock = new Mock<IFormFile>();
                var content = "Sample research document content";
                var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
                fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
                fileMock.Setup(f => f.FileName).Returns("sample_proposal.pdf");
                fileMock.Setup(f => f.Length).Returns(ms.Length);
                fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<System.Threading.CancellationToken>()))
                    .Returns((Stream stream, System.Threading.CancellationToken token) => ms.CopyToAsync(stream, token));

                // Act
                var (storedFileName, fileSize, relativePath) = await storageService.SaveFileAsync(fileMock.Object, "Research_42");

                // Assert
                Assert.NotNull(storedFileName);
                Assert.EndsWith(".pdf", storedFileName);
                Assert.Equal(ms.Length, fileSize);
                Assert.Equal($"uploads/attachments/Research_42/{storedFileName}", relativePath);

                var expectedPhysicalFile = Path.Combine(wwwRoot, "uploads", "attachments", "Research_42", storedFileName);
                Assert.True(File.Exists(expectedPhysicalFile));
            }
            finally
            {
                if (Directory.Exists(tempFolder))
                {
                    try { Directory.Delete(tempFolder, true); } catch { }
                }
            }
        }

        [Fact]
        public void ErrorController_HandleStatusCode_404_LogsAndReturnsNotFoundView()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ErrorController>>();

            var controller = new ErrorController(loggerMock.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/NonExistentPage";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = controller.HandleStatusCode(404) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NotFound", result.ViewName);
            var model = Assert.IsType<ErrorViewModel>(result.Model);
            Assert.NotNull(model.RequestId);
        }

        [Fact]
        public async Task SuperAdminRepository_GetAuditLogsAsync_FiltersByRoleAndStatus()
        {
            // Arrange
            var db = CreateInMemoryDb(Guid.NewGuid().ToString());
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            var roleManagerMock = new Mock<RoleManager<IdentityRole>>(roleStoreMock.Object, null!, null!, null!, null!);
            var emailMock = new Mock<IEmailSender>();

            db.AuditLogs.Add(new AuditLog
            {
                UserId = "user-1",
                UserRole = RoleConstants.SuperAdmin,
                ControllerName = "SuperAdmin",
                ActionName = "CreateCoAdmin",
                HttpMethod = "POST",
                Path = "/SuperAdmin/CreateCoAdmin",
                StatusCode = 200,
                IsSuccess = true,
                Description = "إنشاء مشرف كلية",
                Timestamp = DateTime.UtcNow.AddMinutes(-10)
            });

            db.AuditLogs.Add(new AuditLog
            {
                UserId = "user-2",
                UserRole = RoleConstants.Researcher,
                ControllerName = "Researcher",
                ActionName = "CreateResearch",
                HttpMethod = "POST",
                Path = "/Researcher/CreateResearch",
                StatusCode = 400,
                IsSuccess = false,
                Description = "فشل التحقق من الملفات",
                Timestamp = DateTime.UtcNow.AddMinutes(-5)
            });

            await db.SaveChangesAsync();

            var repo = new SuperAdminRepository(db, userManagerMock.Object, roleManagerMock.Object, emailMock.Object);

            // Act 1 - Filter by Role
            var resultAdmin = await repo.GetAuditLogsAsync(roleFilter: RoleConstants.SuperAdmin);
            Assert.Single(resultAdmin.Logs);
            Assert.Equal(RoleConstants.SuperAdmin, resultAdmin.Logs[0].UserRole);

            // Act 2 - Filter by Success
            var resultFailed = await repo.GetAuditLogsAsync(successFilter: false);
            Assert.Single(resultFailed.Logs);
            Assert.Equal(400, resultFailed.Logs[0].StatusCode);
            Assert.False(resultFailed.Logs[0].IsSuccess);

            // Act 3 - Filter by ActionName
            var resultAction = await repo.GetAuditLogsAsync(actionFilter: "CreateCoAdmin");
            Assert.Single(resultAction.Logs);
            Assert.Equal("CreateCoAdmin", resultAction.Logs[0].ActionName);
        }

        [Fact]
        public async Task SuperAdminRepository_RemoveCoAdminFromCollegeAsync_RemovesEntityAndRole()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            var college = new College { Id = 1, Name = "كلية الهندسة" };
            var user = new ApplicationUser
            {
                Id = "coadmin-user-1",
                UserName = "coadmin@test.edu.eg",
                Email = "coadmin@test.edu.eg",
                FullName = "مهندس الاختبار",
                IsActive = true
            };
            var coAdmin = new CollegeCoAdmin
            {
                Id = 10,
                UserId = "coadmin-user-1",
                User = user,
                CollegeId = 1,
                College = college,
                IsActive = true
            };

            using (var db = new ApplicationDbContext(options))
            {
                db.Colleges.Add(college);
                db.Users.Add(user);
                db.CollegeCoAdmins.Add(coAdmin);
                await db.SaveChangesAsync();
            }

            userManagerMock.Setup(m => m.IsInRoleAsync(It.IsAny<ApplicationUser>(), RoleConstants.CollegeCoAdmin))
                .ReturnsAsync(true);
            userManagerMock.Setup(m => m.RemoveFromRoleAsync(It.IsAny<ApplicationUser>(), RoleConstants.CollegeCoAdmin))
                .ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(m => m.UpdateSecurityStampAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            using (var db = new ApplicationDbContext(options))
            {
                var roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                    new Mock<IRoleStore<IdentityRole>>().Object, null, null, null, null);
                var emailSenderMock = new Mock<IEmailSender>();

                var repo = new SuperAdminRepository(db, userManagerMock.Object, roleManagerMock.Object, emailSenderMock.Object);

                // Act
                var (succeeded, error) = await repo.RemoveCoAdminFromCollegeAsync(10);

                // Assert
                Assert.True(succeeded);
                Assert.Null(error);
                Assert.Empty(db.CollegeCoAdmins);
                userManagerMock.Verify(m => m.RemoveFromRoleAsync(It.IsAny<ApplicationUser>(), RoleConstants.CollegeCoAdmin), Times.Once);
            }
        }
    }
}
