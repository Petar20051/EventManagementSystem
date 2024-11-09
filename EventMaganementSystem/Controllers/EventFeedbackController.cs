using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventMaganementSystem.Controllers
{
    public class EventFeedbackController : Controller
    {
        private readonly IFeedbackService _feedbackService;
        private readonly IEventService _eventService;
        private readonly UserManager<ApplicationUser> _userManager;
        public EventFeedbackController(IFeedbackService feedbackService, IEventService eventService, UserManager<ApplicationUser> userManager)
        {
            _feedbackService = feedbackService;
            _eventService = eventService;
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> Index(int eventId)
        {
            var eventDetails = await _eventService.GetEventDetailsAsync(eventId);
            if (eventDetails == null)
            {
                return NotFound("Event not found.");
            }
            var feedbacks = await _feedbackService.GetFeedbacksByEventIdAsync(eventId);
            var viewModel = new FeedbackViewModel
            {
                EventId = eventId,
                EventName = eventDetails.Name ?? "Event",
                Feedbacks = (List<Feedback>)feedbacks,
                NewFeedback = new Feedback { EventId = eventId }
            };
            ViewData["Title"] = "Event Feedback";
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(FeedbackViewModel model)
        {
            // Remove validation for properties that are set programmatically
            ModelState.Remove("NewFeedback.UserId");
            ModelState.Remove("NewFeedback.Event");
            ModelState.Remove("NewFeedback.User");

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                // Set programmatically-assigned values
                model.NewFeedback.UserId = user.Id;
                model.NewFeedback.FeedbackDate = DateTime.UtcNow;
                model.NewFeedback.EventId = model.EventId;

                // Save the feedback
                await _feedbackService.AddFeedbackAsync(model.NewFeedback);

                // Redirect back to the GET action to display the updated list
                return RedirectToAction(nameof(Index), new { eventId = model.EventId });
            }

            // Reload event details and feedbacks if the model state is invalid
            var eventDetails = await _eventService.GetEventDetailsAsync(model.EventId);
            model.EventName = eventDetails?.Name ?? "Event"; // Ensure EventName is loaded.
            model.Feedbacks = (await _feedbackService.GetFeedbacksByEventIdAsync(model.EventId)).ToList();
            ViewData["Title"] = model.EventName ?? "Event Feedback";

            return View(model);
        }
    }
}
