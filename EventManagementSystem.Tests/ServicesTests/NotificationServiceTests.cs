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
        private readonly Mock<EventDbContext> _mockContext;
        private readonly Mock<INotificationHub> _mockNotificationHub;
        private readonly NotificationService _notificationService;
        private readonly Mock<DbSet<Notification>> _mockDbSet;

        public NotificationServiceTests()
        {
            // Mock the DbSet<Notification>
            _mockDbSet = new Mock<DbSet<Notification>>();

            // Mock the EventDbContext
            _mockContext = new Mock<EventDbContext>();
            _mockContext.Setup(c => c.Notifications).Returns(_mockDbSet.Object);

            // Mock the INotificationHub
            _mockNotificationHub = new Mock<INotificationHub>();

            // Create an instance of NotificationService
            _notificationService = new NotificationService(_mockContext.Object, _mockNotificationHub.Object);
        }

        // Test for CreateNotificationAsync
        [Fact]
        public async Task CreateNotificationAsync_ShouldCreateNotificationAndSendRealTimeNotification()
        {
            // Arrange
            var userId = "user1";
            var message = "Test message";
            var type = NotificationType.NewEventCreated;

            // Act
            await _notificationService.CreateNotificationAsync(userId, message, type);

            // Assert: Check if the notification was added to the DbSet
            _mockDbSet.Verify(m => m.Add(It.IsAny<Notification>()), Times.Once);

            // Assert: Ensure that SaveChangesAsync is called to save the notification
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            // Assert: Ensure real-time notification is sent
            _mockNotificationHub.Verify(n => n.SendNotificationAsync(userId, message, type), Times.Once);
        }

        // Test for NotifyNewEventAsync
        [Fact]
        public async Task NotifyNewEventAsync_ShouldCreateNewEventNotification()
        {
            // Arrange
            var userId = "user1";
            var eventName = "New Event";

            // Act
            await _notificationService.NotifyNewEventAsync(userId, eventName);

            // Assert: Verify that CreateNotificationAsync was called with correct message and type
            _mockNotificationHub.Verify(n => n.SendNotificationAsync(userId, $"A new event, {eventName}, has been added! Check it out now.", NotificationType.NewEventCreated), Times.Once);
        }

        // Test for NotifyEventReminderAsync
        [Fact]
        public async Task NotifyEventReminderAsync_ShouldCreateEventReminderNotification()
        {
            // Arrange
            var userId = "user1";
            var eventName = "Event Reminder";
            var eventTime = DateTime.UtcNow.AddDays(1);

            // Act
            await _notificationService.NotifyEventReminderAsync(userId, eventName, eventTime);

            // Assert: Verify that CreateNotificationAsync was called with correct message and type
            _mockNotificationHub.Verify(n => n.SendNotificationAsync(userId, $"Reminder: The event {eventName} is happening tomorrow at {eventTime}. We look forward to seeing you there!", NotificationType.EventReminder), Times.Once);
        }

        // Test for NotifyGeneralAnnouncementAsync
        [Fact]
        public async Task NotifyGeneralAnnouncementAsync_ShouldCreateGeneralAnnouncementNotification()
        {
            // Arrange
            var userId = "user1";
            var content = "System is down for maintenance.";

            // Act
            await _notificationService.NotifyGeneralAnnouncementAsync(userId, content);

            // Assert: Verify that CreateNotificationAsync was called with correct message and type
            _mockNotificationHub.Verify(n => n.SendNotificationAsync(userId, $"Important Update: {content}", NotificationType.General), Times.Once);
        }

        // Test for NotifySystemAlertAsync
        [Fact]
        public async Task NotifySystemAlertAsync_ShouldCreateSystemAlertNotification()
        {
            // Arrange
            var userId = "user1";
            var alertContent = "Server issue detected.";

            // Act
            await _notificationService.NotifySystemAlertAsync(userId, alertContent);

            // Assert: Verify that CreateNotificationAsync was called with correct message and type
            _mockNotificationHub.Verify(n => n.SendNotificationAsync(userId, $"System Alert: {alertContent}. Please review this information.", NotificationType.SystemAlert), Times.Once);
        }

        // Test for GetUserNotificationsAsync
        [Fact]
        public async Task GetUserNotificationsAsync_ShouldReturnNotificationsForUser()
        {
            // Arrange
            var userId = "user1";
            var notifications = new List<Notification>
        {
            new Notification { UserId = userId, Message = "Test Notification", NotificationDate = DateTime.UtcNow, Type = NotificationType.NewEventCreated },
            new Notification { UserId = userId, Message = "Another Test", NotificationDate = DateTime.UtcNow, Type = NotificationType.EventReminder }
        };

            _mockContext.Setup(m => m.Notifications.Where(n => n.UserId == userId)).Returns(notifications.AsQueryable());

            // Act
            var result = await _notificationService.GetUserNotificationsAsync(userId);

            // Assert
            Assert.Equal(2, result.Count); // Should return two notifications
        }
    }
}
