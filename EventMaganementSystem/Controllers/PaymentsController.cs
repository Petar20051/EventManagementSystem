using Microsoft.AspNetCore.Mvc;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models;
using EventManagementSystem.Infrastructure.Data.Enums;

public class PaymentsController : Controller
{
    private readonly IStripePaymentService _stripePaymentService;
    private readonly IPayPalPaymentService _payPalPaymentService;

    public PaymentsController(IStripePaymentService stripePaymentService, IPayPalPaymentService payPalPaymentService)
    {
        _stripePaymentService = stripePaymentService;
        _payPalPaymentService = payPalPaymentService;
    }

    // Action for choosing a payment method
    [HttpGet]
    public async Task<IActionResult> ChoosePaymentMethod(int reservationId)
    {
        var model = new PaymentViewModel
        {
            ReservationId = reservationId,
            Amount = 100, // example amount
            PaymentFor = PaymentFor.Ticket
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] PaymentViewModel model)
    {
        var sessionId = await _stripePaymentService.CreateCheckoutSession(model.Amount, model.ReservationId);
        return Json(new { id = sessionId });
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] PaymentViewModel model)
    {
        var orderId = await _payPalPaymentService.CreateOrderAsync(model.Amount, model.ReservationId);
        return Json(new { id = orderId });
    }

    [HttpPost]
    public async Task<IActionResult> CaptureOrder(string orderId)
    {
        var success = await _payPalPaymentService.CaptureOrderAsync(orderId);
        return success ? RedirectToAction("PaymentSuccess") : RedirectToAction("PaymentFailed");
    }

    // Action for successful payments
    [HttpGet]
    public IActionResult PaymentSuccess()
    {
        return View();
    }

    // Action for failed payments
    [HttpGet]
    public IActionResult PaymentFailed()
    {
        return View();
    }
}
