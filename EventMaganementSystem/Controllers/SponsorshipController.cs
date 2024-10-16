using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Sponsorship;
using EventManagementSystem.Infrastructure.Data.Enums;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventMaganementSystem.Controllers
{
    public class SponsorshipController : Controller
    {
        private readonly ISponsorshipService _sponsorshipService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPaymentService _paymentService;
        private readonly IStripePaymentService _stripePaymentService;

        public SponsorshipController(ISponsorshipService sponsorshipService, UserManager<ApplicationUser> userManager,
                                     IPaymentService paymentService, IStripePaymentService stripePaymentService)
        {
            _sponsorshipService = sponsorshipService;
            _userManager = userManager;
            _paymentService = paymentService;
            _stripePaymentService = stripePaymentService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessSponsorship(ProcessSponsorshipViewModel model)
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
                // Update user's sponsorship details
                var user = await _userManager.FindByIdAsync(userId);
                user.SponsoredAmount += model.Amount;

                // Update sponsorship tier based on the new sponsorship amount
                await _sponsorshipService.UpdateSponsorshipTierAsync(user, _userManager);

                // Record the payment in the database
                var payment = new Payment
                {
                    UserId = userId,
                    Amount = model.Amount,
                    PaymentMethod = "Stripe", // or any other method
                    PaymentDate = DateTime.Now,
                    Status = "Completed"
                    
                };

                await _paymentService.RecordPaymentAsync(payment);

                TempData["Message"] = $"Thank you for your sponsorship! You are now a {user.SponsorshipTier} sponsor.";
                return RedirectToAction("SponsorDashboard");
            }
            else
            {
                ModelState.AddModelError("", "Payment failed. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> SponsorDashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            var model = new SponsorshipDashboardViewModel
            {
                CurrentTier = user.SponsorshipTier ?? SponsorshipTier.None,
                SponsoredAmount = user.SponsoredAmount,
                Benefits = SponsorshipBenefits.GetBenefits()
                    .Where(b => b.Tier == user.SponsorshipTier)
                    .ToList()
            };

            return View(model);
        }
    }
}
}
