using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace EventMaganementSystem
{
    public class ChatHub:Hub
    {
        private readonly IFeedbackService _feedbackService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatHub(IFeedbackService feedbackService, UserManager<ApplicationUser> userManager)
        {
            _feedbackService = feedbackService;
            _userManager = userManager;
        }

        public async Task SendMessage(int eventId, string message)
        {
            var user = await _userManager.GetUserAsync(Context.User); // Use Context.User instead of UserIdentifier
            if (user == null)
            {
                return;
            }

            // Save the feedback to the database.
            var feedback = new Feedback
            {
                EventId = eventId,
                FeedbackContent = message,
                UserId = user.Id,
                FeedbackDate = DateTime.UtcNow
            };
            await _feedbackService.AddFeedbackAsync(feedback);

            // Notify all connected clients about the new feedback.
            await Clients.Group(eventId.ToString())
                .SendAsync("ReceiveMessage", user.UserName, message, feedback.FeedbackDate);
        }


        public override Task OnConnectedAsync()
        {
            // Automatically add the user to a group for the event.
            var eventId = Context.GetHttpContext().Request.Query["eventId"];
            Groups.AddToGroupAsync(Context.ConnectionId, eventId);

            return base.OnConnectedAsync();
        }
    }
}
