using Microsoft.AspNetCore.Mvc;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models;
using EventManagementSystem.Infrastructure.Data.Enums;
using EventManagementSystem.Infrastructure.Entities;
using EventManagementSystem.Core.Models.Payments;
using System.Security.Claims;
using EventManagementSystem.Core.Services;
using Stripe;
using Microsoft.EntityFrameworkCore;

public class PaymentsController : Controller
{
    private readonly IStripePaymentService _stripePaymentService;
    private readonly IReservationService _reservationService;
    private readonly IPaymentService _paymentService;
    private readonly IUserService _userService;
    private readonly ITicketService _ticketService;
    private readonly IUserEventService _userEventService;

    public PaymentsController(
        IStripePaymentService stripePaymentService,
        IReservationService reservationService,
        IPaymentService paymentService,
        IUserService userService,
        ITicketService ticketService,
        IUserEventService userEventService)
    {
        _stripePaymentService = stripePaymentService;
        _reservationService = reservationService;
        _paymentService = paymentService;
        _userService = userService;
        _ticketService = ticketService;
        _userEventService = userEventService;
    }

    [HttpGet]
    public async Task<IActionResult> ProcessPayment(int reservationId)
    {
        // Fetch the reservation details based on the reservation ID
        var reservation = await _reservationService.GetReservationByIdAsync(reservationId);
        if (reservation == null)
        {
            return NotFound("Reservation not found.");
        }

        // Get the authenticated user's ID
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User is not authenticated.");
        }

        // Fetch the user's stored payment methods (Stripe cards, for example)
        var storedCards = await _stripePaymentService.GetStoredCardsAsync(userId);
        if (storedCards == null || !storedCards.Any())
        {
            ModelState.AddModelError("", "No stored payment methods found. Please add a payment method.");
            return RedirectToAction("AddPaymentMethod", "PaymentMethods");  // Redirect to add payment method if none exist
        }

        // Prepare the view model with the necessary data
        var paymentViewModel = new ProcessPaymentViewModel
        {
            ReservationId = reservationId,
            Amount = reservation.TotalAmount,  // Assuming reservation has TotalAmount
            StoredCards = storedCards  // Pass the user's stored payment methods
        };
        await _userEventService.AddUserEventAsync(userId, reservation.EventId);
        // Return the view with the populated view model
        return View(paymentViewModel);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessPayment(ProcessPaymentViewModel model)
    {
        if (model == null)
        {
            throw new ArgumentNullException(nameof(model), "Model is null in POST");
        }

        if (string.IsNullOrEmpty(model.SelectedCardId))
        {
            ModelState.AddModelError("", "A payment method must be selected.");
            return View(model);
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            ModelState.AddModelError("", "User is not authenticated.");
            return View(model);
        }

        // Process the payment using Stripe
        var paymentStatus = await _stripePaymentService.ProcessPaymentAsync(model.Amount, model.SelectedCardId, userId);

        if (paymentStatus == "succeeded")
        {
            // Fetch the reservation
            var reservation = await _reservationService.GetReservationByIdAsync(model.ReservationId);
            if (reservation == null)
            {
                return NotFound();
            }

            // Update reservation's payment status
            reservation.IsPaid = true;
            reservation.PaymentDate = DateTime.Now;
            await _reservationService.UpdateReservationAsync(reservation);

            // Record the payment in the database
            var payment = new Payment
            {
                ReservationId = reservation.Id,
                UserId = userId,
                Amount = model.Amount,
                PaymentMethod = "Stripe", // or any other method
                PaymentDate = DateTime.Now,
                Status = "Completed"
            };

            await _paymentService.RecordPaymentAsync(payment);

            var ticket = new Ticket
            {
                EventId = reservation.EventId,
                HolderId = reservation.UserId,
                PurchaseDate = DateTime.Now
            };

            await _ticketService.CreateTicketAsync(reservation.EventId, reservation.UserId, DateTime.Now);

            return RedirectToAction("PaymentSuccess");
        }
        else
        {
            ModelState.AddModelError("", "Payment failed. Please try again.");
            return View(model);
        }
    }
    public IActionResult PaymentSuccess()
    {
        ViewBag.Message = "Payment successful!";
        return View();
    }

    public IActionResult PaymentFailed()
    {
        ViewBag.Message = "Payment failed!";
        return View();
    }
}
