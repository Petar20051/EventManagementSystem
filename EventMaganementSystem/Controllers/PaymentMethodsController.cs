using EventManagementSystem.Core;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Payments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Options;
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

        // POST: Handle the submission of a new payment method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPaymentMethod([FromBody] PaymentMethodInputModel inputModel)
        {
            try
            {
                if (inputModel == null || string.IsNullOrEmpty(inputModel.PaymentMethodId))
                {
                    Console.WriteLine("Invalid Payment Method: PaymentMethodId is null or empty.");
                    return BadRequest("Invalid Payment Method");
                }

                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    Console.WriteLine("Unauthorized: User ID not found.");
                    return Unauthorized("User not found.");
                }

                // Fetch or create the Stripe customer ID for the user
                var customerId = await _userService.GetStripeCustomerIdAsync(userId);
                if (string.IsNullOrEmpty(customerId))
                {
                    var user = await _userService.GetUserByIdAsync(userId);
                    if (user == null)
                    {
                        Console.WriteLine("Error: User not found in system.");
                        return BadRequest("User not found in the system.");
                    }

                    // Create a new Stripe customer if customerId is null
                    customerId = await _stripePaymentService.CreateStripeCustomerAsync(userId, user.Email, user.UserName);
                    await _userService.SaveStripeCustomerIdAsync(userId, customerId);
                }

                // Attach the payment method to the customer
                await _stripePaymentService.AttachPaymentMethodAsync(customerId, inputModel.PaymentMethodId);

                return Ok(); // Or Redirect to a success page
            }
            catch (StripeException ex) when (ex.StripeError?.Code == "card_declined")
            {
                Console.WriteLine($"Card Declined: {ex.StripeError.Message}");
                return BadRequest("Your card was declined. Please try a different card or contact your card issuer.");
            }
            catch (StripeException ex)
            {
                Console.WriteLine($"Stripe Error: {ex.StripeError.Message}");
                return StatusCode(500, "An error occurred with Stripe.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }


        // GET: Manage saved payment methods
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            await _paymentService.DeleteCreditCardAsync(cardId, userId);
            return RedirectToAction("ManagePaymentMethods");
        }
    }
}
