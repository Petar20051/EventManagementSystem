using EventMaganementSystem;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Infrastructure.Data.Enums;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem
{
    public class NotificationHubService:INotificationHub
    {
        private readonly IHubContext<ChatHub> _chatHubContext;

        public NotificationHubService(IHubContext<ChatHub> chatHubContext)
        {
            _chatHubContext = chatHubContext;
        }

        public async Task SendNotificationAsync(string userId, string message, NotificationType type)
        {
            await _chatHubContext.Clients.User(userId).SendNotificationAsync("ReceiveNotification", message, type);
        }

        // Implement other methods for each notification type
        public async Task NotifyNewEventAsync(string userId, string eventName)
        {
            string message = $"A new event, {eventName}, has been added! Check it out now.";
            await SendNotificationAsync(userId, message, NotificationType.NewEventCreated);
        }

        public async Task NotifyEventReminderAsync(string userId, string eventName, DateTime eventTime)
        {
            string message = $"Reminder: The event {eventName} is happening tomorrow at {eventTime}. We look forward to seeing you there!";
            await SendNotificationAsync(userId, message, NotificationType.EventReminder);
        }

        public async Task NotifyGeneralAnnouncementAsync(string userId, string content)
        {
            string message = $"Important Update: {content}";
            await SendNotificationAsync(userId, message, NotificationType.General);
        }

        public async Task NotifySystemAlertAsync(string userId, string alertContent)
        {
            string message = $"System Alert: {alertContent}. Please review this information.";
            await SendNotificationAsync(userId, message, NotificationType.SystemAlert);
        }
    }
}
