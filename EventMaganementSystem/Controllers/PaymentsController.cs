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
        
        var reservation = await _reservationService.GetReservationByIdAsync(reservationId);
        if (reservation == null)
        {
            return NotFound("Reservation not found.");
        }

        
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User is not authenticated.");
        }

        
        var storedCards = await _stripePaymentService.GetStoredCardsAsync(userId);
        if (storedCards == null || !storedCards.Any())
        {
            ModelState.AddModelError("", "No stored payment methods found. Please add a payment method.");
            return RedirectToAction("AddPaymentMethod", "PaymentMethods");  
        }

        
        var paymentViewModel = new ProcessPaymentViewModel
        {
            ReservationId = reservationId,
            Amount = reservation.TotalAmount,  
            StoredCards = storedCards  
        };
       
        
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

        
        var paymentStatus = await _stripePaymentService.ProcessPaymentAsync(model.Amount, model.SelectedCardId, userId);

        if (paymentStatus == "succeeded")
        {
           
            var reservation = await _reservationService.GetReservationByIdAsync(model.ReservationId);
            if (reservation == null)
            {
                return NotFound();
            }
            await _userEventService.AddUserEventAsync(userId, reservation.EventId);

            reservation.IsPaid = true;
            reservation.PaymentDate = DateTime.Now;
            await _reservationService.UpdateReservationAsync(reservation);

            
            var payment = new Payment
            {
                ReservationId = reservation.Id,
                UserId = userId,
                Amount = model.Amount,
                PaymentMethod = "Stripe", 
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
            return RedirectToAction("PaymentFailed");
        }
    }

    [HttpGet]
    public async Task<IActionResult> ViewPaymentDetails(int reservationId)
    {
        
        var reservation=await _reservationService.GetReservationByIdAsync(reservationId);
        var paymentDate = reservation.PaymentDate;
        var paymentDetails = await _paymentService.GetPaymentByIdAsync(paymentDate);

        
        if (paymentDetails == null)
        {
            return NotFound();
        }

        return View(paymentDetails); 
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
