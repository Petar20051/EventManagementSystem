using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Services;
using EventManagementSystem.Infrastructure.Data.Enums;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ServicesTests
{
    public class NotificationServiceTests
    {
        private readonly DbContextOptions<EventDbContext> _options;
        private readonly Mock<INotificationHub> _mockNotificationHub;

        public NotificationServiceTests()
        {
            _options = new DbContextOptionsBuilder<EventDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _mockNotificationHub = new Mock<INotificationHub>();
        }

        private NotificationService CreateService(EventDbContext dbContext)
        {
            return new NotificationService(dbContext, _mockNotificationHub.Object);
        }

        private async Task SeedDataAsync(EventDbContext dbContext)
        {
            dbContext.Notifications.AddRange(
                new Notification
                {
                    Id = 1,
                    UserId = "user1",
                    Message = "Test notification 1",
                    NotificationDate = DateTime.UtcNow.AddMinutes(-10),
                    Type = NotificationType.General
                },
                new Notification
                {
                    Id = 2,
                    UserId = "user1",
                    Message = "Test notification 2",
                    NotificationDate = DateTime.UtcNow.AddMinutes(-5),
                    Type = NotificationType.EventReminder
                },
                new Notification
                {
                    Id = 3,
                    UserId = "user2",
                    Message = "Test notification 3",
                    NotificationDate = DateTime.UtcNow.AddMinutes(-1),
                    Type = NotificationType.SystemAlert
                }
            );

            await dbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task NotifyEventReminderAsync_CreatesNotificationAndSendsViaHub()
        {
            using var dbContext = new EventDbContext(_options);
            var service = CreateService(dbContext);

            var userId = "user1";
            var eventName = "Upcoming Event";
            var eventTime = DateTime.UtcNow.AddHours(1);

            // Act
            await service.NotifyEventReminderAsync(userId, eventName, eventTime);

            // Assert
            var notification = await dbContext.Notifications.FirstOrDefaultAsync(n => n.UserId == userId && n.Type == NotificationType.EventReminder);
            Assert.NotNull(notification);
            Assert.Equal($"Reminder: The event {eventName} is happening tomorrow at {eventTime}. We look forward to seeing you there!", notification.Message);

            _mockNotificationHub.Verify(h => h.SendNotificationAsync(
                userId,
                $"Reminder: The event {eventName} is happening tomorrow at {eventTime}. We look forward to seeing you there!",
                NotificationType.EventReminder), Times.Once);
        }

        // Update other tests similarly to ensure the same context and service scope.
        [Fact]
        public async Task CreateNotificationAsync_AddsNotificationAndSendsViaHub()
        {
            using var dbContext = new EventDbContext(_options);
            var service = CreateService(dbContext);

            var userId = "user1";
            var message = "Test notification";
            var type = NotificationType.General;

            // Act
            await service.CreateNotificationAsync(userId, message, type);

            // Assert
            var notification = await dbContext.Notifications.FirstOrDefaultAsync(n => n.Message == message);
            Assert.NotNull(notification);
            Assert.Equal(userId, notification.UserId);
            Assert.Equal(type, notification.Type);

            _mockNotificationHub.Verify(h => h.SendNotificationAsync(userId, message, type), Times.Once);
        }

        [Fact]
        public async Task GetUserNotificationsAsync_ReturnsNotificationsForUser()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);
            var service = CreateService(dbContext);

            // Act
            var notifications = await service.GetUserNotificationsAsync("user1");

            // Assert
            Assert.Equal(2, notifications.Count);
            Assert.Equal("Test notification 2", notifications.First().Message); // Ordered by NotificationDate DESC
        }
    }
}
