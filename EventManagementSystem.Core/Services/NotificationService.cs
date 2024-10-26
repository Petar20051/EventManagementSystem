using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Infrastructure.Data.Enums;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNet.SignalR;
using Microsoft.EntityFrameworkCore;
using PayPalCheckoutSdk.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Services
{
   public class NotificationService:INotificationService
    {
        private readonly EventDbContext _context;
        private readonly INotificationHub _notificationHub;

        public NotificationService(EventDbContext context, INotificationHub notificationHub)
        {
            _context = context;
            _notificationHub = notificationHub;
        }

        public async Task CreateNotificationAsync(string userId, string message, NotificationType type)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                NotificationDate = DateTime.UtcNow,
                Type = type
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send the notification in real-time via INotificationHub
            await _notificationHub.SendNotificationAsync(userId, message, type);
        }

        public async Task NotifyNewEventAsync(string userId, string eventName)
        {
            await _notificationHub.NotifyNewEventAsync(userId, eventName);
        }

        public async Task NotifyEventReminderAsync(string userId, string eventName, DateTime eventTime)
        {
            await _notificationHub.NotifyEventReminderAsync(userId, eventName, eventTime);
        }

        public async Task NotifyGeneralAnnouncementAsync(string userId, string content)
        {
            await _notificationHub.NotifyGeneralAnnouncementAsync(userId, content);
        }

        public async Task NotifySystemAlertAsync(string userId, string alertContent)
        {
            await _notificationHub.NotifySystemAlertAsync(userId, alertContent);
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId)
        {
            return await _context.Notifications
                                 .Where(n => n.UserId == userId)
                                 .OrderByDescending(n => n.NotificationDate)
                                 .ToListAsync();
        }
    }
}
