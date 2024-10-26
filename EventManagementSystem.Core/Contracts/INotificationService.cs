using EventManagementSystem.Infrastructure.Data.Enums;
using EventManagementSystem.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Contracts
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string userId, string message, NotificationType type);
        Task NotifyNewEventAsync(string userId, string eventName);
        Task NotifyEventReminderAsync(string userId, string eventName, DateTime eventTime);
        Task NotifyGeneralAnnouncementAsync(string userId, string content);
        Task NotifySystemAlertAsync(string userId, string alertContent);
        Task<List<Notification>> GetUserNotificationsAsync(string userId);
    }
}
