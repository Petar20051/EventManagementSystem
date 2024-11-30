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

        // POST: Handle the submission of a new payment method
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

                var customerId = await _userService.GetStripeCustomerIdAsync(userId);
                if (string.IsNullOrEmpty(customerId))
                {
                    var user = await _userService.GetUserByIdAsync(userId);
                    if (user == null)
                    {
                        return BadRequest(new { error = "User not found in the system." });
                    }

                    customerId = await _stripePaymentService.CreateStripeCustomerAsync(userId, user.Email, user.UserName);
                    await _userService.SaveStripeCustomerIdAsync(userId, customerId);
                }

                var paymentMethodService = new PaymentMethodService();
                var paymentMethod = await paymentMethodService.GetAsync(inputModel.PaymentMethodId);

                if (paymentMethod.Customer?.Id == customerId)
                {
                    return Ok(new { message = "Payment method already added." });
                }
                else if (!string.IsNullOrEmpty(paymentMethod.Customer?.Id))
                {
                    return BadRequest(new { error = "Payment method is already associated with another account." });
                }

                await _stripePaymentService.AttachPaymentMethodAsync(customerId, inputModel.PaymentMethodId);

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
                Console.WriteLine($"Stripe Error Code: {ex.StripeError?.Code}");
                Console.WriteLine($"Stripe Error Message: {ex.StripeError?.Message}");
                return BadRequest(new { error = ex.StripeError?.Message ?? "Stripe error occurred." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled Exception: {ex.Message}");
                return StatusCode(500, new { error = "An unexpected error occurred." });
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
