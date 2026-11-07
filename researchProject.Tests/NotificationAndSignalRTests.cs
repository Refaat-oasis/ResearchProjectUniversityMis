using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using researchProject.Controllers;
using researchProject.Data;
using researchProject.Hubs;
using researchProject.Models.Entities;
using researchProject.Services.Implementations;
using researchProject.Services.Interfaces;
using Xunit;

namespace researchProject.Tests
{
    public class NotificationAndSignalRTests
    {
        private ApplicationDbContext CreateInMemoryDb(string name)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(name)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ApplicationDbContext(options);
        }

        private (Mock<IHubContext<NotificationHub>> HubMock, Mock<IClientProxy> ClientMock) CreateHubMock()
        {
            var hubContextMock = new Mock<IHubContext<NotificationHub>>();
            var hubClientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();

            clientProxyMock.Setup(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            hubClientsMock.Setup(c => c.User(It.IsAny<string>())).Returns(clientProxyMock.Object);
            hubClientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);
            hubContextMock.Setup(h => h.Clients).Returns(hubClientsMock.Object);

            return (hubContextMock, clientProxyMock);
        }

        [Fact]
        public async Task SendNotificationAsync_ValidParams_PersistsNotificationAndDispatchesToSignalR()
        {
            // Arrange
            using var db = CreateInMemoryDb("SendNotification_PersistsAndDispatches");
            var (hubMock, clientMock) = CreateHubMock();
            var service = new NotificationService(db, hubMock.Object);

            string userId = "user-123";
            string title = "تكليف بتحكيم بحث جديد";
            string message = "تم إسناد مقترح بحثي إليكم لتحكيمه";
            string targetUrl = "/Judge/Evaluate/10";
            string type = "JudgeAssignment";

            // Act
            await service.SendNotificationAsync(userId, title, message, targetUrl, type, "bi-mortarboard");

            // Assert
            var saved = await db.Notifications.FirstOrDefaultAsync(n => n.UserId == userId);
            Assert.NotNull(saved);
            Assert.Equal(title, saved.Title);
            Assert.Equal(message, saved.Message);
            Assert.Equal(targetUrl, saved.TargetUrl);
            Assert.Equal(type, saved.Type);
            Assert.Equal("bi-mortarboard", saved.Icon);
            Assert.False(saved.IsRead);

            // Verify SignalR dispatch
            clientMock.Verify(c => c.SendCoreAsync(
                "ReceiveNotification",
                It.Is<object[]>(args => args.Length == 1),
                It.IsAny<CancellationToken>()), Times.AtLeastOnce());
        }

        [Fact]
        public async Task GetUserNotificationsAsync_ReturnsOrderedWithArabicTimeAgo()
        {
            // Arrange
            using var db = CreateInMemoryDb("GetUserNotifications_ArabicTimeAgo");
            var (hubMock, _) = CreateHubMock();
            var service = new NotificationService(db, hubMock.Object);

            string userId = "user-test";
            db.Notifications.AddRange(
                new Notification
                {
                    UserId = userId,
                    Title = "إشعار قديم",
                    Message = "رسالة قديمة",
                    TargetUrl = "/Researcher/Tracker/1",
                    Type = "StatusChange",
                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                    IsRead = false
                },
                new Notification
                {
                    UserId = userId,
                    Title = "إشعار حديث",
                    Message = "رسالة حديثة",
                    TargetUrl = "/Researcher/Tracker/2",
                    Type = "StatusChange",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                    IsRead = false
                }
            );
            await db.SaveChangesAsync();

            // Act
            var results = await service.GetUserNotificationsAsync(userId, 10);

            // Assert
            Assert.Equal(2, results.Count);
            Assert.Equal("إشعار حديث", results[0].Title); // descending order
            Assert.Contains("دقيقة", results[0].TimeAgo);
            Assert.Contains("ساعة", results[1].TimeAgo);
        }

        [Fact]
        public async Task UnreadCountAndMarkAsRead_CorrectlyUpdatesState()
        {
            // Arrange
            using var db = CreateInMemoryDb("UnreadCountAndMarkAsRead");
            var (hubMock, _) = CreateHubMock();
            var service = new NotificationService(db, hubMock.Object);

            string userId = "user-abc";
            var n1 = new Notification { UserId = userId, Title = "T1", Message = "M1", IsRead = false };
            var n2 = new Notification { UserId = userId, Title = "T2", Message = "M2", IsRead = false };
            var n3 = new Notification { UserId = userId, Title = "T3", Message = "M3", IsRead = true };
            db.Notifications.AddRange(n1, n2, n3);
            await db.SaveChangesAsync();

            // Act & Assert 1: Initial unread count should be 2
            var initialUnread = await service.GetUnreadCountAsync(userId);
            Assert.Equal(2, initialUnread);

            // Act & Assert 2: Mark one as read
            var markResult = await service.MarkAsReadAsync(n1.Id, userId);
            Assert.True(markResult);
            var countAfterOne = await service.GetUnreadCountAsync(userId);
            Assert.Equal(1, countAfterOne);

            // Act & Assert 3: Mark all as read
            var markAllResult = await service.MarkAllAsReadAsync(userId);
            Assert.True(markAllResult);
            var finalCount = await service.GetUnreadCountAsync(userId);
            Assert.Equal(0, finalCount);
        }

        [Fact]
        public async Task MarkAsReadAsync_DifferentUser_ReturnsFalseAndPreservesState()
        {
            // Arrange
            using var db = CreateInMemoryDb("MarkAsRead_UnauthorizedUser");
            var (hubMock, _) = CreateHubMock();
            var service = new NotificationService(db, hubMock.Object);

            var notif = new Notification { UserId = "owner-user", Title = "T", Message = "M", IsRead = false };
            db.Notifications.Add(notif);
            await db.SaveChangesAsync();

            // Act: Hacker tries to mark owner's notification
            var result = await service.MarkAsReadAsync(notif.Id, "attacker-user");

            // Assert
            Assert.False(result);
            var reloaded = await db.Notifications.FindAsync(notif.Id);
            Assert.NotNull(reloaded);
            Assert.False(reloaded.IsRead);
        }

        [Fact]
        public async Task NotificationController_Endpoints_ReturnCorrectJsonResults()
        {
            // Arrange
            var mockService = new Mock<INotificationService>();
            mockService.Setup(s => s.GetUnreadCountAsync("test-user")).ReturnsAsync(5);
            mockService.Setup(s => s.MarkAsReadAsync(99, "test-user")).ReturnsAsync(true);
            mockService.Setup(s => s.MarkAllAsReadAsync("test-user")).ReturnsAsync(true);

            var controller = new NotificationController(mockService.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user")
            }, "TestAuth"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act 1: GetUnreadCount
            var countResult = await controller.GetUnreadCount() as JsonResult;
            Assert.NotNull(countResult);

            // Act 2: MarkAsRead
            var markResult = await controller.MarkAsRead(99) as JsonResult;
            Assert.NotNull(markResult);

            // Act 3: MarkAllAsRead
            var markAllResult = await controller.MarkAllAsRead() as JsonResult;
            Assert.NotNull(markAllResult);
        }
    }
}

