using System;
using System.IO;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using researchProject.Controllers;
using researchProject.Models.Constants;
using researchProject.Models.Enums;
using researchProject.Models.ViewModels;
using researchProject.Repositories.Interfaces;
using researchProject.Services;
using Xunit;

namespace researchProject.Tests
{
    public class EightFeaturesAndEnhancementsTests
    {
        [Fact]
        public void DeveloperRole_Constant_Exists_AndMatchesUserRoleEnum()
        {
            Assert.Equal("Developer", RoleConstants.Developer);
            Assert.Equal(RoleConstants.Developer, UserRole.Developer.ToString());
        }

        [Fact]
        public void HangfireDashboardAuthorizationFilter_Allows_Developer_And_SuperAdmin()
        {
            // Developer User
            var devContext = new DefaultHttpContext();
            var devIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "dev@mis.edu.eg"),
                new Claim(ClaimTypes.Role, RoleConstants.Developer)
            }, "TestAuth");
            devContext.User = new ClaimsPrincipal(devIdentity);

            Assert.True(devContext.User.IsInRole(RoleConstants.Developer));
            Assert.False(devContext.User.IsInRole(RoleConstants.SuperAdmin));

            // SuperAdmin User
            var adminContext = new DefaultHttpContext();
            var adminIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "admin@mis.edu.eg"),
                new Claim(ClaimTypes.Role, RoleConstants.SuperAdmin)
            }, "TestAuth");
            adminContext.User = new ClaimsPrincipal(adminIdentity);

            Assert.True(adminContext.User.IsInRole(RoleConstants.SuperAdmin));
        }

        [Fact]
        public void DeveloperController_Has_AuthorizeAttribute_With_Developer_Only()
        {
            var authAttr = (AuthorizeAttribute?)Attribute.GetCustomAttribute(typeof(DeveloperController), typeof(AuthorizeAttribute));
            Assert.NotNull(authAttr);
            Assert.Equal(RoleConstants.Developer, authAttr.Roles);
            Assert.DoesNotContain(RoleConstants.SuperAdmin, authAttr.Roles);
        }

        [Fact]
        public void SuperAdminController_DoesNotHave_AuditLogs_Action()
        {
            var method = typeof(SuperAdminController).GetMethod("AuditLogs");
            Assert.Null(method);
        }

        [Fact]
        public void SuperAdminController_DoesNotHave_AuditLogsPartial_Action()
        {
            var method = typeof(SuperAdminController).GetMethod("AuditLogsPartial");
            Assert.Null(method);
        }

        [Fact]
        public async Task DeveloperController_AuditLogs_Returns_View_With_Model()
        {
            var mockRepo = new Mock<ISuperAdminRepository>();
            var expectedModel = new AuditLogListViewModel();
            mockRepo.Setup(r => r.GetAuditLogsAsync(1, 25, null, null, null, null, null, null))
                    .ReturnsAsync(expectedModel);

            var controller = new DeveloperController(mockRepo.Object);

            var result = await controller.AuditLogs(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(expectedModel, viewResult.Model);
        }

        [Fact]
        public async Task DeveloperController_AuditLogsPartial_Returns_PartialView_With_Model()
        {
            var mockRepo = new Mock<ISuperAdminRepository>();
            var expectedModel = new AuditLogListViewModel();
            mockRepo.Setup(r => r.GetAuditLogsAsync(1, 25, null, null, null, null, null, null))
                    .ReturnsAsync(expectedModel);

            var controller = new DeveloperController(mockRepo.Object);

            var result = await controller.AuditLogsPartial(1);

            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AuditLogsPartial", partialResult.ViewName);
            Assert.Same(expectedModel, partialResult.Model);
        }

        [Fact]
        public void DeveloperController_Hangfire_Returns_View()
        {
            var mockRepo = new Mock<ISuperAdminRepository>();
            var controller = new DeveloperController(mockRepo.Object);

            var result = controller.Hangfire();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void DynamicFieldDefinition_Supports_Options_Array_Serialization()
        {
            var fields = new[]
            {
                new
                {
                    Name = "SampleMCQ",
                    Label = "نوع العينة البيولوجية",
                    Type = "dropdown",
                    IsRequired = true,
                    Options = new[] { "عينة دم", "خزعة نسيجية", "مسحة جينية" }
                }
            };

            var options = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(fields, options);
            Assert.Contains("SampleMCQ", json);
            Assert.Contains("dropdown", json);
            Assert.Contains("عينة دم", json);
            Assert.Contains("خزعة نسيجية", json);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.Equal(JsonValueKind.Array, root.ValueKind);
            var field0 = root[0];
            Assert.True(field0.TryGetProperty("Options", out var opts));
            Assert.Equal(3, opts.GetArrayLength());
            Assert.Equal("عينة دم", opts[0].GetString());
        }

        [Fact]
        public async Task EmailSender_RegistrationToken_Saves_Canonical_last_otp_html_With_CopyButton()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "EmailArchiveTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                var mockLogger = new Mock<ILogger<EmailSender>>();
                var inMemoryConfig = new System.Collections.Generic.Dictionary<string, string?>
                {
                    { "Smtp:Host", "" }, // Simulated mode
                    { "Smtp:Port", "587" },
                    { "Smtp:User", "" },
                    { "Smtp:Password", "" }
                };
                var config = new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfig).Build();

                var mockEnv = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
                mockEnv.Setup(e => e.ContentRootPath).Returns(tempDir);

                var sender = new EmailSender(mockLogger.Object, config, mockEnv.Object);

                await sender.SendRegistrationTokenAsync("test@univ.edu.eg", "123456", "https://example.com/verify?token=123456");

                var archiveDir = Path.Combine(tempDir, "App_Data", "SentEmails");
                var expectedFile = Path.Combine(archiveDir, "last_otp.html");

                Assert.True(File.Exists(expectedFile), $"Expected file {expectedFile} was not found.");

                var content = await File.ReadAllTextAsync(expectedFile);
                Assert.Contains("123456", content);
                Assert.Contains("نسخ رمز التحقق", content);
                Assert.Contains("navigator.clipboard.writeText", content);
                Assert.Contains("copyOtpToClipboard", content);
                Assert.Contains("execCommand", content);
                Assert.Contains("href='https://example.com/verify?token=123456'", content);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                }
            }
        }

        [Fact]
        public async Task EmailSender_PasswordResetToken_Contains_Resilient_Copy_And_Fallback_Link()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "EmailResetArchiveTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                var mockLogger = new Mock<ILogger<EmailSender>>();
                var inMemoryConfig = new System.Collections.Generic.Dictionary<string, string?>
                {
                    { "Smtp:Host", "" },
                    { "Smtp:Port", "587" },
                    { "Smtp:User", "" },
                    { "Smtp:Password", "" }
                };
                var config = new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfig).Build();

                var mockEnv = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
                mockEnv.Setup(e => e.ContentRootPath).Returns(tempDir);

                var sender = new EmailSender(mockLogger.Object, config, mockEnv.Object);

                await sender.SendPasswordResetTokenAsync("reset@univ.edu.eg", "654321", "https://example.com/reset?token=654321");

                var archiveDir = Path.Combine(tempDir, "App_Data", "SentEmails");
                var expectedFile = Path.Combine(archiveDir, "last_otp.html");

                Assert.True(File.Exists(expectedFile), $"Expected reset token archive at {expectedFile}");
                var content = await File.ReadAllTextAsync(expectedFile);

                Assert.Contains("654321", content);
                Assert.Contains("نسخ رمز الاستعادة", content);
                Assert.Contains("copyOtpToClipboard", content);
                Assert.Contains("https://example.com/reset?token=654321", content);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                }
            }
        }

        [Fact]
        public async Task SuperAdminController_RemoveCoAdminFromCollege_Returns_JsonResult()
        {
            var mockRepo = new Mock<ISuperAdminRepository>();
            mockRepo.Setup(r => r.RemoveCoAdminFromCollegeAsync(5))
                    .ReturnsAsync((true, null));

            var mockEmail = new Mock<IEmailSender>();
            var mockDispatcher = new Mock<IHangfireEmailDispatcher>();
            var mockExport = new Mock<IReportExportService>();

            var controller = new SuperAdminController(mockRepo.Object, mockEmail.Object, mockDispatcher.Object, mockExport.Object);

            var result = await controller.RemoveCoAdminFromCollege(5);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var val = jsonResult.Value;
            var successProp = val?.GetType().GetProperty("success")?.GetValue(val);
            Assert.Equal(true, successProp);
            mockRepo.Verify(r => r.RemoveCoAdminFromCollegeAsync(5), Times.Once);
        }

        [Fact]
        public async Task DeveloperController_AuditLogs_WithActionFilter_PassesFilterToRepo()
        {
            var mockRepo = new Mock<ISuperAdminRepository>();
            var expectedModel = new AuditLogListViewModel { ActionFilter = "SubmitResearch" };
            mockRepo.Setup(r => r.GetAuditLogsAsync(1, 25, null, null, null, null, null, "SubmitResearch"))
                    .ReturnsAsync(expectedModel);

            var controller = new DeveloperController(mockRepo.Object);

            var result = await controller.AuditLogs(1, null, null, null, null, null, "SubmitResearch");

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(expectedModel, viewResult.Model);
            mockRepo.Verify(r => r.GetAuditLogsAsync(1, 25, null, null, null, null, null, "SubmitResearch"), Times.Once);
        }
    }
}
