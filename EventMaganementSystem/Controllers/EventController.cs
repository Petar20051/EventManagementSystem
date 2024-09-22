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
                Date = e.Date,
                Description = e.Description,
                Location = e.Venue != null ? e.Venue.Name : "Unknown Location"  
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


    }
}
