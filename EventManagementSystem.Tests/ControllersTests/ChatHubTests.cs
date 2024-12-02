using EventMaganementSystem;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Infrastructure.Data.Enums;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ControllersTests
{
    public class ChatHubTests
    {
        private readonly Mock<IHubCallerClients> _clientsMock;
        private readonly Mock<IClientProxy> _clientProxyMock;
        private readonly Mock<IFeedbackService> _feedbackServiceMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly ChatHub _chatHub;

        public ChatHubTests()
        {
            _clientsMock = new Mock<IHubCallerClients>();
            _clientProxyMock = new Mock<IClientProxy>();
            _feedbackServiceMock = new Mock<IFeedbackService>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            _clientsMock.Setup(clients => clients.User(It.IsAny<string>())).Returns(_clientProxyMock.Object);

            _chatHub = new ChatHub(_feedbackServiceMock.Object, _userManagerMock.Object)
            {
                Clients = _clientsMock.Object
            };
        }

        [Fact]
        public async Task SendNotificationAsync_SendsMessageToSpecificUser()
        {
            // Arrange
            var userId = "test_user";
            var message = "Test notification message";
            var type = NotificationType.General;

            _clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync(
                    "ReceiveNotification",
                    It.Is<object[]>(args => args[0].ToString() == message &&
                                            args[1].ToString() == type.ToString()),
                    default))
                .Returns(Task.CompletedTask);

            // Act
            await _chatHub.SendNotificationAsync(userId, message, type);

            // Assert
            _clientProxyMock.Verify(proxy => proxy.SendCoreAsync(
                "ReceiveNotification",
                It.Is<object[]>(args => args[0].ToString() == message &&
                                        args[1].ToString() == type.ToString()),
                default), Times.Once);
        }

        [Fact]
        public async Task NotifyNewEventAsync_SendsNewEventNotification()
        {
            // Arrange
            var userId = "test_user";
            var eventName = "Test Event";

            var expectedMessage = $"A new event, {eventName}, has been added! Check it out now.";

            _clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync(
                    "ReceiveNotification",
                    It.Is<object[]>(args => args[0].ToString() == expectedMessage &&
                                            args[1].ToString() == NotificationType.NewEventCreated.ToString()),
                    default))
                .Returns(Task.CompletedTask);

            // Act
            await _chatHub.NotifyNewEventAsync(userId, eventName);

            // Assert
            _clientProxyMock.Verify(proxy => proxy.SendCoreAsync(
                "ReceiveNotification",
                It.Is<object[]>(args => args[0].ToString() == expectedMessage &&
                                        args[1].ToString() == NotificationType.NewEventCreated.ToString()),
                default), Times.Once);
        }

        [Fact]
        public async Task NotifyEventReminderAsync_SendsReminderNotification()
        {
            // Arrange
            var userId = "test_user";
            var eventName = "Test Event";
            var eventTime = DateTime.Now.AddDays(1);

            var expectedMessage = $"Reminder: The event {eventName} is happening tomorrow at {eventTime}. We look forward to seeing you there!";

            _clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync(
                    "ReceiveNotification",
                    It.Is<object[]>(args => args[0].ToString() == expectedMessage &&
                                            args[1].ToString() == NotificationType.EventReminder.ToString()),
                    default))
                .Returns(Task.CompletedTask);

            // Act
            await _chatHub.NotifyEventReminderAsync(userId, eventName, eventTime);

            // Assert
            _clientProxyMock.Verify(proxy => proxy.SendCoreAsync(
                "ReceiveNotification",
                It.Is<object[]>(args => args[0].ToString() == expectedMessage &&
                                        args[1].ToString() == NotificationType.EventReminder.ToString()),
                default), Times.Once);
        }

        [Fact]
        public async Task NotifyGeneralAnnouncementAsync_SendsGeneralNotification()
        {
            // Arrange
            var userId = "test_user";
            var content = "Important update message.";
            var expectedMessage = $"Important Update: {content}";

            _clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync(
                    "ReceiveNotification",
                    It.Is<object[]>(args => args[0].ToString() == expectedMessage &&
                                            args[1].ToString() == NotificationType.General.ToString()),
                    default))
                .Returns(Task.CompletedTask);

            // Act
            await _chatHub.NotifyGeneralAnnouncementAsync(userId, content);

            // Assert
            _clientProxyMock.Verify(proxy => proxy.SendCoreAsync(
                "ReceiveNotification",
                It.Is<object[]>(args => args[0].ToString() == expectedMessage &&
                                        args[1].ToString() == NotificationType.General.ToString()),
                default), Times.Once);
        }

        [Fact]
        public async Task NotifySystemAlertAsync_SendsSystemAlertNotification()
        {
            // Arrange
            var userId = "test_user";
            var alertContent = "System alert content.";
            var expectedMessage = $"System Alert: {alertContent}. Please review this information.";

            _clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync(
                    "ReceiveNotification",
                    It.Is<object[]>(args => args[0].ToString() == expectedMessage &&
                                            args[1].ToString() == NotificationType.SystemAlert.ToString()),
                    default))
                .Returns(Task.CompletedTask);

            // Act
            await _chatHub.NotifySystemAlertAsync(userId, alertContent);

            // Assert
            _clientProxyMock.Verify(proxy => proxy.SendCoreAsync(
                "ReceiveNotification",
                It.Is<object[]>(args => args[0].ToString() == expectedMessage &&
                                        args[1].ToString() == NotificationType.SystemAlert.ToString()),
                default), Times.Once);
        }
    }
}
