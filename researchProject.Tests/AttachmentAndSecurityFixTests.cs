using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using researchProject.Controllers;
using researchProject.Data;
using researchProject.Models.Constants;
using researchProject.Models.DTOs;
using researchProject.Models.Entities;
using researchProject.Services;
using researchProject.Services.Implementations;
using researchProject.Services.Interfaces;
using Xunit;

namespace researchProject.Tests
{
    public class AttachmentAndSecurityFixTests
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
        public async Task AttachmentDownload_SuperAdmin_CanAccessAnyAttachment()
        {
            // Arrange
            var db = CreateInMemoryDb(Guid.NewGuid().ToString());
            var userManagerMock = CreateUserManagerMock();
            var fileStorageMock = new Mock<IFileStorageService>();

            var superAdminUser = new ApplicationUser { Id = "super-1", UserName = "admin@uni.edu" };
            userManagerMock.Setup(m => m.FindByIdAsync("super-1")).ReturnsAsync(superAdminUser);
            userManagerMock.Setup(m => m.IsInRoleAsync(superAdminUser, RoleConstants.SuperAdmin)).ReturnsAsync(true);

            var college = new College { Id = 1, Name = "Medicine", Code = "MED" };
            var research = new Research { Id = 10, Title = "Cancer Study", CollegeId = 1 };
            var attachment = new ResearchAttachment
            {
                Id = 101,
                ResearchId = 10,
                OriginalFileName = "study.pdf",
                StoredFileName = "stored_study.pdf",
                AttachmentType = AttachmentTypeConstants.ResearchProposal,
                Version = 1,
                FileSize = 1024
            };

            await db.Colleges.AddAsync(college);
            await db.Researches.AddAsync(research);
            await db.ResearchAttachments.AddAsync(attachment);
            await db.SaveChangesAsync();

            var fakeStream = new MemoryStream(Encoding.UTF8.GetBytes("fake file content"));
            fileStorageMock.Setup(f => f.GetFileStreamAsync("stored_study.pdf", "Research_10"))
                .ReturnsAsync(fakeStream);

            var attachmentService = new AttachmentService(db, fileStorageMock.Object, userManagerMock.Object);

            // Act
            var (isAuthorized, fileExists, fileStream, contentType, fileName) = await attachmentService.GetAttachmentDownloadAsync(101, "super-1");

            // Assert
            Assert.True(isAuthorized);
            Assert.True(fileExists);
            Assert.NotNull(fileStream);
            Assert.Equal("study.pdf", fileName);
            Assert.Equal("application/pdf", contentType);
        }

        [Fact]
        public async Task AttachmentDownload_UnauthorizedUser_IsDenied()
        {
            // Arrange
            var db = CreateInMemoryDb(Guid.NewGuid().ToString());
            var userManagerMock = CreateUserManagerMock();
            var fileStorageMock = new Mock<IFileStorageService>();

            var strangerUser = new ApplicationUser { Id = "stranger-1", UserName = "stranger@external.com" };
            userManagerMock.Setup(m => m.FindByIdAsync("stranger-1")).ReturnsAsync(strangerUser);
            userManagerMock.Setup(m => m.IsInRoleAsync(strangerUser, It.IsAny<string>())).ReturnsAsync(false);

            var college = new College { Id = 1, Name = "Medicine", Code = "MED" };
            var research = new Research { Id = 10, Title = "Cancer Study", CollegeId = 1 };
            var attachment = new ResearchAttachment
            {
                Id = 101,
                ResearchId = 10,
                OriginalFileName = "study.pdf",
                StoredFileName = "stored_study.pdf",
                AttachmentType = AttachmentTypeConstants.ResearchProposal,
                Version = 1
            };

            await db.Colleges.AddAsync(college);
            await db.Researches.AddAsync(research);
            await db.ResearchAttachments.AddAsync(attachment);
            await db.SaveChangesAsync();

            var attachmentService = new AttachmentService(db, fileStorageMock.Object, userManagerMock.Object);

            // Act
            var (isAuthorized, fileExists, fileStream, contentType, fileName) = await attachmentService.GetAttachmentDownloadAsync(101, "stranger-1");

            // Assert
            Assert.False(isAuthorized);
            Assert.Null(fileStream);
        }

        [Fact]
        public async Task AttachmentController_Download_ReturnsFileResultWhenAuthorized()
        {
            // Arrange
            var attachmentServiceMock = new Mock<IAttachmentService>();
            var fakeStream = new MemoryStream(Encoding.UTF8.GetBytes("test"));
            attachmentServiceMock.Setup(s => s.GetAttachmentDownloadAsync(101, "user-1"))
                .ReturnsAsync((true, true, fakeStream, "application/pdf", "proposal.pdf"));

            var controller = new AttachmentController(attachmentServiceMock.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-1"),
                new Claim(ClaimTypes.Role, RoleConstants.Researcher)
            }, "TestAuth"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.Download(101);

            // Assert
            var fileResult = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.Equal("proposal.pdf", fileResult.FileDownloadName);
        }

        [Fact]
        public async Task AttachmentController_Download_ReturnsForbidWhenNotAuthorized()
        {
            // Arrange
            var attachmentServiceMock = new Mock<IAttachmentService>();
            attachmentServiceMock.Setup(s => s.GetAttachmentDownloadAsync(101, "user-1"))
                .ReturnsAsync((false, false, null, "", ""));

            var controller = new AttachmentController(attachmentServiceMock.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-1")
            }, "TestAuth"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.Download(101);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public void VerifyEmailDto_TrimsLeadingAndTrailingSpacesAutomatically()
        {
            // Arrange & Act
            var dto = new VerifyEmailDto
            {
                EmailOrUserId = "  test@domain.com  ",
                Token = "  654321  \n"
            };

            // Assert
            Assert.Equal("654321", dto.Token);
        }
    }
}

