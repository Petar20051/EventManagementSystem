using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Reservation;
using EventManagementSystem.Core.Services;
using EventManagementSystem.Infrastructure.Data.Enums;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventMaganementSystem.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly IEventService _eventService;
        private readonly IStripePaymentService _stripePaymentService;
        private readonly IUserService _userService;
        private readonly IDiscountService _discountService;
        private readonly IProfileService _profileService;
        public ReservationsController(IReservationService reservationService, IEventService eventService, IStripePaymentService stripePaymentService,IUserService userService, IDiscountService discountService, IProfileService profileService)
        {
            _reservationService = reservationService;
            _eventService = eventService;
            _stripePaymentService = stripePaymentService;
            _userService = userService;
            _discountService = discountService;
            _profileService = profileService;
        }

        
        [HttpGet]
        public async Task<IActionResult> Create(int? eventId = null)
        {
            var events = await _eventService.GetAllEventsAsync();
            var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userid == null) return Redirect("/Identity/Account/Login");
            var viewModel = new ReservationViewModel
            {
                Events = events,
                EventId = eventId,
                ReservationDate = DateTime.Now
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservationViewModel viewModel)
        {
            var reservatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!ModelState.IsValid)
            {
                viewModel.Events = await _eventService.GetAllEventsAsync();
                return View(viewModel);
            }

            if (viewModel.EventId == null || viewModel.EventId <= 0)
            {
                ModelState.AddModelError("", "Please select a valid event.");
                viewModel.Events = await _eventService.GetAllEventsAsync();
                return View(viewModel);
            }

            var currentEvent = await _eventService.GetEventByIdAsync(viewModel.EventId.Value);
            if (currentEvent == null)
            {
                ModelState.AddModelError("", "Selected event does not exist.");
                viewModel.Events = await _eventService.GetAllEventsAsync();
                return View(viewModel);
            }

            var existingReservations = await _reservationService.GetAllReservationsAsync();
            bool exist = existingReservations.Any(r => r.UserId == reservatorId && r.EventId == viewModel.EventId.Value);

            if (exist)
            {
                TempData["ErrorMessage"] = "You already have a reservation for this event.";
                return RedirectToAction("Index");
            }

            try
            {
                var reservation = new Reservation
                {
                    EventId = viewModel.EventId.Value,
                    AttendeesCount = viewModel.AttendeesCount,
                    ReservationDate = DateTime.Now,
                    UserId = reservatorId,
                    TotalAmount = currentEvent.TicketPrice * viewModel.AttendeesCount
                };

                await _reservationService.CreateReservationAsync(reservation);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                viewModel.Events = await _eventService.GetAllEventsAsync();
                return RedirectToAction("MainError", "Home", new { message = ex.Message });
            }

            return RedirectToAction("Index");
        }





        public async Task<IActionResult> Index()
        {
            var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userid == null) return Redirect("/Identity/Account/Login");
            
            var reservations = await _reservationService.GetAllReservationsAsync();
            var reservator = await _userService.GetUserByIdAsync(userid);
            var Reservations= reservations.Where(r=> r.UserId == userid);

            var reservationViewModels = new List<ReservationViewModel>();

            
            foreach (var reservation in Reservations)
            {
                
                decimal totalAmount = reservation.Event.TicketPrice * reservation.AttendeesCount;
                decimal discountedAmount = await _discountService.ApplyDiscountAsync(reservator.SponsorshipTier, totalAmount);

                
                var viewModel = new ReservationViewModel
                {
                    Id = reservation.Id,
                    EventId = reservation.EventId,
                    EventName = reservation.Event.Name,
                    AttendeesCount = reservation.AttendeesCount,
                    ReservationDate = reservation.ReservationDate,
                    IsPaid = reservation.IsPaid,
                    TotalAmount = totalAmount,
                    DiscountedAmount = discountedAmount
                };

                reservationViewModels.Add(viewModel);
            }

            
            return View(reservationViewModels);
        }


        public async Task<IActionResult> Delete(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);

            if (reservation == null)
            {
                return NotFound();
            }

            var @event = await _eventService.GetEventByIdAsync(reservation.EventId);
            if (@event != null)
            {
                reservation.Event.Name = @event.Name;
            }

           
            if (reservation.IsPaid) 
            {
                TempData["ErrorMessage"] = "Cannot delete a reservation that has already been paid.";
                return RedirectToAction("Index");
            }

            return View(reservation);
        }



        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _reservationService.DeleteReservationAsync(id);
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Edit(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);

            if (reservation == null)
            {
                return NotFound();
            }

            
            if (reservation.IsPaid) 
            {
                TempData["ErrorMessage"] = "Cannot edit a reservation that has already been paid.";
                return RedirectToAction("Index"); 
            }

            var events = await _eventService.GetAllEventsAsync();
            var viewModel = new ReservationViewModel
            {
                Id = reservation.Id,
                EventId = reservation.EventId,
                AttendeesCount = reservation.AttendeesCount,
                ReservationDate = reservation.ReservationDate,
                Events = events
            };

            return View(viewModel);
        }



        [HttpPost]
        public async Task<IActionResult> Edit(ReservationViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                
                viewModel.Events = await _eventService.GetAllEventsAsync();
                return View(viewModel);
            }


            
            var existingReservation = await _reservationService.GetReservationByIdAsync((int)viewModel.Id);
            if (existingReservation == null)
            {
                return NotFound();
            }

            
            existingReservation.EventId = (int)viewModel.EventId;
            existingReservation.AttendeesCount = viewModel.AttendeesCount;
            existingReservation.ReservationDate = viewModel.ReservationDate;

            
            await _reservationService.UpdateReservationAsync(existingReservation);

            return RedirectToAction("Index"); 
        }
        public async Task<IActionResult> CreatePayment(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);

            
            if (reservation == null)
            {
                return NotFound(); 
            }

            
            return View(reservation); 
        }

        

        [HttpGet]
        public IActionResult PaymentSuccess()
        {
            
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
