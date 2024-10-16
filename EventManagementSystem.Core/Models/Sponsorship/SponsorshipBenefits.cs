using EventManagementSystem.Infrastructure.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Models.Sponsorship
{
    public static class SponsorshipBenefits
    {
        public static List<SponsorshipBenefit> GetBenefits()
        {
            return new List<SponsorshipBenefit>
            {
                new SponsorshipBenefit
                {
                    Tier = SponsorshipTier.Bronze,
                    Description = "Discounts for tickets or event-related services",
                    MinimumSponsorshipAmount = 1 // Example amount
                },
                new SponsorshipBenefit
                {
                    Tier = SponsorshipTier.Silver,
                    Description = "Logo displayed in the event brochure and other marketing materials",
                    MinimumSponsorshipAmount = 20 // Example amount
                },
                new SponsorshipBenefit
                {
                    Tier = SponsorshipTier.Gold,
                    Description = "Logo in brochures + Social media shoutout + VIP seating",
                    MinimumSponsorshipAmount = 100 // Example amount
                }
            };
        }
    }
}
