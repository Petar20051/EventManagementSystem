using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Events;
using EventManagementSystem.Core.Models.Venue;
using EventManagementSystem.Core.Services;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventMaganementSystem.Controllers
{
    public class EventsController : Controller
    {
        private readonly IEventService _eventService;
        private readonly IVenueService _venueService;
        private readonly  IUserEventService _userEventService;

        public EventsController(IEventService eventService, IVenueService venueService,IUserEventService userEventService)
        {
            _eventService = eventService;
            _venueService = venueService;
            _userEventService = userEventService;

        }

        public async Task<IActionResult> Index()
        {
            var eventItems = await _eventService.GetAllEventsAsync();
            var eventViewModels = eventItems.Select(e => new EventViewModel
            {
                Id = e.Id,
                Name = e.Name,
                Date = e.Date
                
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
                    OrganizerId = organizerId
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

        public async Task<IActionResult> Edit(int id)
        {
            var eventItem = await _eventService.GetEventByIdAsync(id);
            if (eventItem == null) return NotFound();

            var model = new EventViewModel
            {
                Id = eventItem.Id,
                Name = eventItem.Name,
               
                Date = eventItem.Date,
             
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EventViewModel model)
        {
            if (ModelState.IsValid)
            {
                var eventItem = new Event
                {
                    Id = model.Id,
                    Name = model.Name,
                    
                    Date = model.Date
                    
                };
                await _eventService.UpdateEventAsync(eventItem);
                TempData["Message"] = "Event updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _eventService.DeleteEventAsync(id);
            TempData["Message"] = "Event deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
