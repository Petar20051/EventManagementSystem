using EventManagementSystem.Core;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Payments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace EventMaganementSystem.Controllers
{
    public class PaymentMethodsController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IStripePaymentService _stripePaymentService;
        private readonly IUserService _userService;
        private readonly StripeSettings _stripeOptions;

        public PaymentMethodsController(IPaymentService paymentService, IStripePaymentService stripePaymentService, IUserService userService, IOptions<StripeSettings> stripeOptions)
        {
            _paymentService = paymentService;
            _stripePaymentService = stripePaymentService;
            _userService = userService;
            _stripeOptions = stripeOptions.Value;
        }

        // GET: Display form to add a credit card
        [HttpGet]
        public IActionResult AddPaymentMethod()
        {
            // Replace with your actual Stripe publishable key
            ViewBag.PublishableKey = _stripeOptions.PublishableKey;
            return View();
        }

        // POST: Handle the submission of a new payment method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPaymentMethod([FromBody] PaymentMethodInputModel inputModel)
        {
            if (inputModel == null || string.IsNullOrEmpty(inputModel.PaymentMethodId))
            {
                return BadRequest("Invalid Payment Method");
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("User not found.");
            }

            // Fetch the Stripe customer ID for the user, or create a new one
            var customerId = await _userService.GetStripeCustomerIdAsync(userId);
            if (string.IsNullOrEmpty(customerId))
            {
                var user = await _userService.GetUserByIdAsync(userId);
                customerId = await _stripePaymentService.CreateStripeCustomerAsync(userId, user.Email, user.UserName);
            }

            // Attach the payment method to the customer
            await _stripePaymentService.AttachPaymentMethodAsync(customerId, inputModel.PaymentMethodId);

            return Ok(); // Or Redirect to a success page
        }

        // GET: Manage saved payment methods
        public async Task<IActionResult> ManagePaymentMethods()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;

            // Fetch the user's stored payment methods
            var storedCards = await _paymentService.GetUserPaymentMethodsAsync(userId);

            // Pass the stored payment methods to the view
            return View(storedCards);
        }

        // POST: Delete a payment method (e.g., credit card)
        [HttpPost]
        public async Task<IActionResult> DeletePaymentMethod(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized(); // Ensure user is authenticated
            }

            // Delete the selected payment method
            await _paymentService.DeleteCreditCardAsync(id, userId);

            // Redirect back to the manage payment methods page
            return RedirectToAction("ManagePaymentMethods");
        }
    }
}
