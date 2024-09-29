using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Reservation;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventMaganementSystem.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly IEventService _eventService;

        public ReservationsController(IReservationService reservationService, IEventService eventService)
        {
            _reservationService = reservationService;
            _eventService = eventService;
        }

        // GET: Reservations/Create
        public async Task<IActionResult> Create()
        {
            var events = await _eventService.GetAllEventsAsync();
            var viewModel = new ReservationViewModel
            {
                Events = events // Assuming events is a List<Event>
            };

            return View(viewModel);
        }

        // POST: Reservations/Create
        [HttpPost]
        public async Task<IActionResult> Create(ReservationViewModel viewModel)
        {
            var reservatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!ModelState.IsValid)
            {
                // Repopulate events in case of error
                viewModel.Events = await _eventService.GetAllEventsAsync();
                return View(viewModel);
            }

            // Create reservation from view model
            var reservation = new Reservation
            {
                EventId = viewModel.EventId,
                AttendeesCount = viewModel.AttendeesCount,
                ReservationDate = DateTime.Now,
                UserId = reservatorId
                // You can set other properties here as needed
            };

            await _reservationService.CreateReservationAsync(reservation);

            return RedirectToAction("Index"); // Redirect to the index or another view after creation
        }

        // GET: Reservations/Index
        public async Task<IActionResult> Index()
        {
            var reservations = await _reservationService.GetAllReservationsAsync();
            
            return View(reservations);
        }

        // GET: Reservations/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            var @event = await _eventService.GetEventByIdAsync(reservation.EventId);
            if (reservation == null)
            {
                return NotFound();
            }
            reservation.Event.Name = @event.Name;

            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _reservationService.DeleteReservationAsync(id);
            return RedirectToAction("Index");
        }

        // GET: Reservations/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            var events = await _eventService.GetAllEventsAsync();
            var viewModel = new ReservationViewModel
            {
                Id = reservation.Id,
                EventId = reservation.EventId,
                AttendeesCount = reservation.AttendeesCount,
                ReservationDate = reservation.ReservationDate,
                Events = events // Populate the list of events
            };

            return View(viewModel);
        }

        // POST: Reservations/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(ReservationViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate events in case of error
                viewModel.Events = await _eventService.GetAllEventsAsync();
                return View(viewModel);
            }


            // Fetch the existing reservation
            var existingReservation = await _reservationService.GetReservationByIdAsync((int)viewModel.Id);
            if (existingReservation == null)
            {
                return NotFound();
            }

            // Update the reservation properties
            existingReservation.EventId = viewModel.EventId;
            existingReservation.AttendeesCount = viewModel.AttendeesCount;
            existingReservation.ReservationDate = viewModel.ReservationDate;

            // Call the service to update the reservation
            await _reservationService.UpdateReservationAsync(existingReservation);

            return RedirectToAction("Index"); // Redirect to the index or another view after editing
        }
    }
}
