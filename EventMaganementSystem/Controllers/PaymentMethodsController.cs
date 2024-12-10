using EventManagementSystem.Core;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Payments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Stripe;
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
            var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userid == null) return Redirect("/Identity/Account/Login");
            // Replace with your actual Stripe publishable key
            ViewBag.PublishableKey = _stripeOptions.PublishableKey;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPaymentMethod([FromBody] PaymentMethodInputModel inputModel)
        {
            try
            {
                if (inputModel == null || string.IsNullOrEmpty(inputModel.PaymentMethodId))
                {
                    return BadRequest(new { error = "Invalid Payment Method." });
                }

                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "User not found." });
                }

                // Fetch stored cards for the user
                var storedCards = await _stripePaymentService.GetStoredCardsAsync(userId);

                // Check if the card already exists
                var paymentMethodService = new PaymentMethodService();
                var newPaymentMethod = await paymentMethodService.GetAsync(inputModel.PaymentMethodId);

                bool alreadyExists = storedCards.Any(card =>
                    card.Last4Digits == newPaymentMethod.Card.Last4 );

                if (alreadyExists)
                {
                    return BadRequest(new { error = "This card is already added to your account." });
                }

                // Attach the payment method to the user
                var customerId = await _userService.GetStripeCustomerIdAsync(userId);
                await _stripePaymentService.AttachPaymentMethodAsync(customerId, inputModel.PaymentMethodId);

                // Set the payment method as default
                var customerService = new CustomerService();
                await customerService.UpdateAsync(customerId, new CustomerUpdateOptions
                {
                    InvoiceSettings = new CustomerInvoiceSettingsOptions
                    {
                        DefaultPaymentMethod = inputModel.PaymentMethodId
                    }
                });

                return Ok(new { message = "Payment method added successfully." });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.StripeError?.Message ?? "Stripe error occurred." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        public async Task<IActionResult> ManagePaymentMethods()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the logged-in user ID
            var paymentMethods = await _stripePaymentService.GetStoredCardsAsync(userId);

            return View(paymentMethods);
        }

        // POST: Delete a payment method (e.g., credit card)
        [HttpPost]
        public async Task<IActionResult> DeletePaymentMethod(string cardId)
        {
            // Get the current user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                // Call the service to detach the payment method
                await _paymentService.DeleteCreditCardAsync(cardId, userId);

                // Add a success message to TempData
                TempData["Message"] = "Payment method deleted successfully.";
            }
            catch (StripeException ex)
            {
                // Handle Stripe-specific exceptions
                TempData["Error"] = $"Stripe error: {ex.Message}";
            }
            catch (InvalidOperationException ex)
            {
                // Handle custom service-layer errors
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                TempData["Error"] = "An unexpected error occurred while deleting the payment method. Please try again.";
                // Optionally log the error for debugging
                // _logger.LogError(ex, "Error deleting payment method for user {UserId}", userId);
            }

            // Redirect back to the Manage Payment Methods page
            return RedirectToAction("ManagePaymentMethods");
        }

    }
}
