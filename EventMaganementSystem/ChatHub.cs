using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Infrastructure.Data.Enums;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace EventMaganementSystem
{
    public class ChatHub : Hub, INotificationHub
    {
        private readonly IFeedbackService _feedbackService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatHub(IFeedbackService feedbackService, UserManager<ApplicationUser> userManager)
        {
            _feedbackService = feedbackService;
            _userManager = userManager;
        }

      
        public async Task SendNotificationAsync(string userId, string message, NotificationType type)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message, type);
        }

        
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

        
        public override async Task OnConnectedAsync()
        {
            var eventId = Context.GetHttpContext().Request.Query["eventId"];
            if (!string.IsNullOrEmpty(eventId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, eventId);
            }
            await base.OnConnectedAsync();
        }
    }
}
