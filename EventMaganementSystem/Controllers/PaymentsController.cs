using EventManagementSystem.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using EventManagementSystem.Core.Models;
using EventManagementSystem.Infrastructure.Data.Enums;

namespace EventMaganementSystem.Controllers
{


    public class PaymentsController : Controller
    {
        private readonly IStripePaymentService _stripePaymentService;
        private readonly IPayPalPaymentService _payPalPaymentService;
        private readonly IReservationService _reservationService;

        public PaymentsController(IStripePaymentService stripePaymentService, IPayPalPaymentService payPalPaymentService,IReservationService reservationService)
        {
            _stripePaymentService = stripePaymentService;
            _payPalPaymentService = payPalPaymentService;
            _reservationService = reservationService;
        }

        // Action for choosing a payment method
        [HttpGet]
        public async Task<IActionResult> ChoosePaymentMethod(int reservationId)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(reservationId);

            // Ensure that reservation is not null before accessing its properties
            if (reservation == null)
            {
                // Return a 404 Not Found response if the reservation does not exist
                return NotFound($"Reservation with ID {reservationId} not found.");
            }

            var model = new PaymentViewModel
            {
                ReservationId = reservationId,
                Amount = reservation.TotalAmount, // Ensure TotalAmount is a property of your Reservation class
                PaymentFor = PaymentFor.Ticket // Use the desired payment type
            };

            return View(model); // Return view for choosing PayPal or Stripe
        }


        // Action for processing the payment method
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(PaymentViewModel model, string paymentMethod)
        {
            if (string.IsNullOrEmpty(paymentMethod))
            {
                return View("PaymentFailed"); // Return failure if no payment method is provided
            }

            if (paymentMethod == "Stripe")
            {
                var stripeSession = await _stripePaymentService.CreateStripeSession(model.Amount, model.UserId, model.ReservationId, model.PaymentFor);
                return Redirect(stripeSession.Url); // Redirect to Stripe checkout
            }
            else if (paymentMethod == "PayPal")
            {
                var payPalPayment = await _payPalPaymentService.CreatePayPalPaymentAsync(model.Amount, "USD", model.UserId, model.ReservationId, model.PaymentFor);
                var approvalUrl = payPalPayment.links.FirstOrDefault(l => l.rel == "approval_url")?.href;

                if (approvalUrl != null)
                {
                    return Redirect(approvalUrl); // Redirect to PayPal approval
                }
                else
                {
                    return View("PaymentFailed"); // If no approval URL is found
                }
            }

            return View("PaymentFailed"); // Handle invalid payment method
        }

        // Action for successful payments
        [HttpGet]
        public IActionResult PaymentSuccess()
        {
            return View(); // Show success message
        }

        // Action for failed payments
        [HttpGet]
        public IActionResult PaymentFailed()
        {
            return View(); // Show failed payment message
        }
    }
}
