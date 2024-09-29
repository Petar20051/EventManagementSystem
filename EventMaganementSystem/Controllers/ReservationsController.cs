using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Reservation;
using EventManagementSystem.Infrastructure.Data.Enums;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
using PayPal.Api;
using System.Security.Claims;

namespace EventMaganementSystem.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly IEventService _eventService;
        private readonly IPayPalPaymentService _payPalPaymentService;
        private readonly IStripePaymentService _stripePaymentService;

        public ReservationsController(IReservationService reservationService, IEventService eventService, IPayPalPaymentService payPalPaymentService, IStripePaymentService stripePaymentService)
        {
            _reservationService = reservationService;
            _eventService = eventService;
            _payPalPaymentService = payPalPaymentService;
            _stripePaymentService = stripePaymentService;
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
            
            var currentEvent = await _eventService.GetEventByIdAsync(reservation.EventId);
            reservation.TotalAmount = currentEvent.TicketPrice * reservation.AttendeesCount;
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
        public async Task<IActionResult> CreatePayment(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);

            // Check if the reservation exists
            if (reservation == null)
            {
                return NotFound(); // Return a 404 Not Found if the reservation does not exist
            }

            // Here you might want to include additional logic to set up the payment
            return View(reservation); // Return the view with the reservation details
        }

        [HttpPost]
        public async Task<IActionResult> Pay(int reservationId, string paymentMethod)
        {
            var reservatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservation = await _reservationService.GetReservationByIdAsync(reservationId);
            if (reservation == null) return NotFound();

            if (paymentMethod == "paypal")
            {
                var approvalUrl = await _payPalPaymentService.CreatePayPalPaymentAsync(10,"USD",reservatorId,reservationId,PaymentFor.Ticket);
               return RedirectToAction("Index");

            }
            else if (paymentMethod == "stripe")
            {
                
                var chargeId = await _stripePaymentService.CreateStripeSession(10, reservatorId,reservationId, PaymentFor.Ticket );
                reservation.IsPaid = true; // Mark as paid
                await _reservationService.UpdateReservationAsync(reservation); // Update reservation
                return RedirectToAction("PaymentSuccess");
            }

            return RedirectToAction("PaymentCancel");
        }

        [HttpGet]
        public IActionResult PaymentSuccess()
        {
            // Optionally, you can pass any message or reservation details here
            ViewBag.Message = "Payment completed successfully!";
            return View();
        }

        [HttpGet]
        public IActionResult PaymentCancel()
        {
            ViewBag.Message = "Payment was canceled. Please try again.";
            return View();
        }
    }
}
