using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Events;
using EventManagementSystem.Core.Models.Venue;
using EventManagementSystem.Core.Services;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EventMaganementSystem.Controllers
{
    public class EventsController : Controller
    {
        private readonly IEventService _eventService;
        private readonly IVenueService _venueService;
        private readonly  IUserEventService _userEventService;
        private readonly IAttendeeService _attendeeService;

        public EventsController(IEventService eventService, IVenueService venueService,IUserEventService userEventService, IAttendeeService attendeeService)
        {
            _eventService = eventService;
            _venueService = venueService;
            _userEventService = userEventService;
            _attendeeService = attendeeService;
        }

        public async Task<IActionResult> Index()
        {
            var eventItems = await _eventService.GetAllEventsAsync();
            ViewBag.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var eventViewModels = eventItems.Select(e => new EventViewModel
            {
                Id = e.Id,
                Name = e.Name,
                Date = e.Date,
                Description = e.Description,
                Location = e.Venue != null ? e.Venue.Address : "Unknown Location"  
            }).ToList();

            return View(eventViewModels);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var venues = await _venueService.GetAllVenuesAsync();
            var model = new CreateEventViewModel
            {
                Venues = venues.Select(venue => new VenueViewModel
                {
                    Id = venue.Id,
                    Name = venue.Name
                }).ToList()
            };
            model.Date = DateTime.Now;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEventViewModel model)
        {
           

            if (ModelState.IsValid)
            {
                var organizerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var eventItem = new Event
                {
                    Name = model.Name,
                    Description = model.Description,
                    Date = model.Date,
                    VenueId = (int)model.VenueId,
                    OrganizerId = organizerId,
                    TicketPrice = model.TicketPrice,

                };

                await _eventService.AddEventAsync(eventItem);

                var userEvent = new UserEvent
                {
                    UserId = organizerId,
                    EventId = eventItem.Id
                };

                await _userEventService.AddUserEventAsync(userEvent);
                TempData["Message"] = "Event created successfully.";
                return RedirectToAction(nameof(Index));
            }

            var venues = await _venueService.GetAllVenuesAsync();
            model.Venues = venues.Select(venue => new VenueViewModel
            {
                Id = venue.Id,
                Name = venue.Name
            }).ToList();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var eventItem = await _eventService.GetEventByIdAsync(id);
            if (eventItem == null) return NotFound();
            var organizerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var model = new EventViewModel
            {
                Id = eventItem.Id,
                Name = eventItem.Name,
                Date = eventItem.Date,
                Description = eventItem.Description,
                VenueId = eventItem.VenueId, 
                OrganizerId = organizerId
            };

            var venues = await _venueService.GetAllVenuesAsync();
            model.Venues = venues.Select(venue => new VenueViewModel
            {
                Id = venue.Id,
                Name = venue.Name
            }).ToList();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EventViewModel model)
        {
            if (ModelState.IsValid)
            {
                var eventItem = await _eventService.GetEventByIdAsync(model.Id);
                if (eventItem == null) return NotFound();

                eventItem.Name = model.Name;
                eventItem.Description = model.Description;
                eventItem.Date = model.Date;
                eventItem.VenueId = model.VenueId;

                await _eventService.UpdateEventAsync(eventItem);
                TempData["Message"] = "Event updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            // Repopulate venues if the model state is invalid
            var venues = await _venueService.GetAllVenuesAsync();
            model.Venues = venues.Select(venue => new VenueViewModel
            {
                Id = venue.Id,
                Name = venue.Name
            }).ToList();

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            
            var hasTickets = await _eventService.HasTicketsAsync(id);
            if (hasTickets)
            {
                TempData["ErrorMessage"] = "Cannot delete the event because there are tickets associated with it.";
                return RedirectToAction(nameof(Index));
            }
            await _eventService.DeleteEventAsync(id);
            TempData["Message"] = "Event deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            // Fetch the event
            var eventDetails = await _eventService.GetEventByIdAsync(id);

            // Check if the event was found
            if (eventDetails == null)
            {
                // Return a NotFound view or an appropriate response
                return NotFound("Event not found");
            }

            // Check if the venue is null
            var venueName = eventDetails.Venue != null ? eventDetails.Venue.Name : "Venue information not available";

            // Create the ViewModel
            var model = new EventDetailsViewModel
            {
                Id = eventDetails.Id,
                Name = eventDetails.Name,
                Date = eventDetails.Date,
                Description = eventDetails.Description,
                Location = venueName,  // Assign venue name or a default message
                OrganizerId = eventDetails.OrganizerId
            };

            // Return the view with the model
            return View(model);
        }


        public async Task<IActionResult> AttendeeList(int eventId)
        {
            // Get the organizer's ID (logged-in user's ID)
            var organizerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Use the service to get the attendee list
            var attendees = await _attendeeService.GetAttendeesForEventAsync(eventId, organizerId);

            if (attendees == null)
            {
                return NotFound(); // Event not found or user is not the organizer
            }

            return View(attendees); // Pass the attendees list to the view
        }

    }
}
