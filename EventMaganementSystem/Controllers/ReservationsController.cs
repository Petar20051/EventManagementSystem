﻿using EventManagementSystem.Core.Contracts;
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

        // GET: Reservations/Create/{eventId}
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

        // POST: Reservations/Create
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

            var reservation = new Reservation
            {
                EventId = viewModel.EventId ?? 0,
                AttendeesCount = viewModel.AttendeesCount,
                ReservationDate = DateTime.Now,
                UserId = reservatorId
            };

            var currentEvent = await _eventService.GetEventByIdAsync(viewModel.EventId ?? 0);
            if (currentEvent == null)
            {
                ModelState.AddModelError("", "Selected event does not exist.");
                viewModel.Events = await _eventService.GetAllEventsAsync();
                return View(viewModel);
            }

            reservation.TotalAmount = currentEvent.TicketPrice * viewModel.AttendeesCount;
            await _reservationService.CreateReservationAsync(reservation);

            return RedirectToAction("Index");
        }



        // GET: Reservations/Index
        public async Task<IActionResult> Index()
        {
            var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userid == null) return Redirect("/Identity/Account/Login");
            // Fetch reservations from the service (this returns a list of Reservation entities)
            var reservations = await _reservationService.GetAllReservationsAsync();
            var reservator = await _userService.GetUserByIdAsync(userid);
            var Reservations= reservations.Where(r=> r.UserId == userid);

            var reservationViewModels = new List<ReservationViewModel>();

            // Process each reservation asynchronously
            foreach (var reservation in Reservations)
            {
                // Calculate total amount and apply discount
                decimal totalAmount = reservation.Event.TicketPrice * reservation.AttendeesCount;
                decimal discountedAmount = await _discountService.ApplyDiscountAsync(reservator.SponsorshipTier, totalAmount);

                // Map to ReservationViewModel
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

            // Pass the ViewModel list to the view
            return View(reservationViewModels);
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
            existingReservation.EventId = (int)viewModel.EventId;
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
