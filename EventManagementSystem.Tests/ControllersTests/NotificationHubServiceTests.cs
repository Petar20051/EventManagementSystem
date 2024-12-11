using EventMaganementSystem;
using EventManagementSystem.Infrastructure.Data.Enums;
using Microsoft.AspNetCore.SignalR;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ControllersTests
{
    public class NotificationHubServiceTests
    {
        private readonly Mock<IHubContext<ChatHub>> _hubContextMock;
        private readonly Mock<IHubClients> _clientsMock;
        private readonly Mock<IClientProxy> _clientProxyMock;
        private readonly NotificationHubService _notificationHubService;

        public NotificationHubServiceTests()
        {
            _hubContextMock = new Mock<IHubContext<ChatHub>>();
            _clientsMock = new Mock<IHubClients>();
            _clientProxyMock = new Mock<IClientProxy>();

            _hubContextMock.Setup(hub => hub.Clients).Returns(_clientsMock.Object);
            _clientsMock.Setup(clients => clients.User(It.IsAny<string>())).Returns(_clientProxyMock.Object);

            _notificationHubService = new NotificationHubService(_hubContextMock.Object);
        }

        [Fact]
        public async Task SendNotificationAsync_SendsMessageToSpecificUser()
        {
            
            var userId = "test_user";
            var message = "Test notification message";
            var type = NotificationType.General;

            _clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync(
                    "ReceiveNotification",
                    It.Is<object[]>(args => args[0].ToString() == message && args[1].Equals(type)),
                    default))
                .Returns(Task.CompletedTask);

            
            await _notificationHubService.SendNotificationAsync(userId, message, type);

                        _clientProxyMock.Verify(proxy => proxy.SendCoreAsync(
                "ReceiveNotification",
                It.Is<object[]>(args => args[0].ToString() == message && args[1].Equals(type)),
                default), Times.Once);
        }

        [Fact]
        public async Task NotifyNewEventAsync_SendsNewEventNotification()
        {
                        var userId = "test_user";
            var eventName = "Test Event";
            var expectedMessage = $"A new event, {eventName}, has been added! Check it out now.";

            _clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync(
                    "ReceiveNotification",
                    It.Is<object[]>(args => args[0].ToString() == expectedMessage && args[1].Equals(NotificationType.NewEventCreated)),
                    default))
                .Returns(Task.CompletedTask);

                        await _notificationHubService.NotifyNewEventAsync(userId, eventName);

                        _clientProxyMock.Verify(proxy => proxy.SendCoreAsync(
                "ReceiveNotification",
                It.Is<object[]>(args => args[0].ToString() == expectedMessage && args[1].Equals(NotificationType.NewEventCreated)),
                default), Times.Once);
        }

        [Fact]
        public async Task NotifyEventReminderAsync_SendsEventReminderNotification()
        {
                        var userId = "test_user";
            var eventName = "Test Event";
            var eventTime = DateTime.Now.AddDays(1);
            var expectedMessage = $"Reminder: The event {eventName} is happening tomorrow at {eventTime}. We look forward to seeing you there!";

            _clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync(
                    "ReceiveNotification",
                    It.Is<object[]>(args => args[0].ToString() == expectedMessage && args[1].Equals(NotificationType.EventReminder)),
                    default))
                .Returns(Task.CompletedTask);

                        await _notificationHubService.NotifyEventReminderAsync(userId, eventName, eventTime);

                        _clientProxyMock.Verify(proxy => proxy.SendCoreAsync(
                "ReceiveNotification",
                It.Is<object[]>(args => args[0].ToString() == expectedMessage && args[1].Equals(NotificationType.EventReminder)),
                default), Times.Once);
        }

        [Fact]
        public async Task NotifyGeneralAnnouncementAsync_SendsGeneralAnnouncementNotification()
        {
                        var userId = "test_user";
            var content = "Important update message.";
            var expectedMessage = $"Important Update: {content}";

            _clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync(
                    "ReceiveNotification",
                    It.Is<object[]>(args => args[0].ToString() == expectedMessage && args[1].Equals(NotificationType.General)),
                    default))
                .Returns(Task.CompletedTask);

                        await _notificationHubService.NotifyGeneralAnnouncementAsync(userId, content);

                        _clientProxyMock.Verify(proxy => proxy.SendCoreAsync(
                "ReceiveNotification",
                It.Is<object[]>(args => args[0].ToString() == expectedMessage && args[1].Equals(NotificationType.General)),
                default), Times.Once);
        }

        [Fact]
        public async Task NotifySystemAlertAsync_SendsSystemAlertNotification()
        {
                        var userId = "test_user";
            var alertContent = "System alert content.";
            var expectedMessage = $"System Alert: {alertContent}. Please review this information.";

            _clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync(
                    "ReceiveNotification",
                    It.Is<object[]>(args => args[0].ToString() == expectedMessage && args[1].Equals(NotificationType.SystemAlert)),
                    default))
                .Returns(Task.CompletedTask);

                        await _notificationHubService.NotifySystemAlertAsync(userId, alertContent);

                        _clientProxyMock.Verify(proxy => proxy.SendCoreAsync(
                "ReceiveNotification",
                It.Is<object[]>(args => args[0].ToString() == expectedMessage && args[1].Equals(NotificationType.SystemAlert)),
                default), Times.Once);
        }
    }
}
