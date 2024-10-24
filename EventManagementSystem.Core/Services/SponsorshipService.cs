using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Infrastructure.Data.Enums;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Services
{
    public class SponsorshipService:ISponsorshipService
    {
        private readonly IPaymentService _paymentService;
        private readonly IStripePaymentService _stripePaymentService;

        public SponsorshipService(IPaymentService paymentService, IStripePaymentService stripePaymentService)
        {
            _paymentService = paymentService;
            _stripePaymentService = stripePaymentService;
        }

        // Process the sponsorship payment
        public async Task ProcessSponsorshipAsync(ApplicationUser user, decimal amount, string selectedCardId)
        {
            // Process the payment using the StripePaymentService
            var paymentStatus = await _stripePaymentService.ProcessPaymentAsync(amount, selectedCardId, user.Id);

            if (paymentStatus == "succeeded")
            {
                // Add to user's sponsored amount
                user.SponsoredAmount += amount;
            }
            else
            {
                throw new Exception("Payment failed. Please try again.");
            }
        }

        // Update the sponsorship tier based on the total sponsored amount
        public async Task UpdateSponsorshipTierAsync(ApplicationUser user, UserManager<ApplicationUser> userManager)
        {
            var newTier = DetermineSponsorshipTier(user.SponsoredAmount);
            if (user.SponsorshipTier != newTier)
            {
                user.SponsorshipTier = newTier;
                await userManager.UpdateAsync(user); // Update the user in the database
            }
        }

        // Determine sponsorship tier based on the sponsored amount
        public SponsorshipTier DetermineSponsorshipTier(decimal sponsoredAmount)
        {
            if (sponsoredAmount >= 1)
            {
                return SponsorshipTier.Bronze;
            }
            if (sponsoredAmount >= 20)
            {
                return SponsorshipTier.Silver;
            }
            if (sponsoredAmount >= 100)
            {
                return SponsorshipTier.Gold;
            }
            return SponsorshipTier.None;
        }
    }
}

