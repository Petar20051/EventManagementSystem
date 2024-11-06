using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Infrastructure.Data.Enums;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Services
{
    public class DiscountService : IDiscountService
    {
        public async Task<decimal> ApplyDiscountAsync(SponsorshipTier? sponsorRank, decimal originalPrice)
        {
            int discountPercentage = GetSponsorDiscountPercentage(sponsorRank);
            decimal discountAmount = (originalPrice * discountPercentage) / 100;
            return originalPrice - discountAmount;
        }

        private int GetSponsorDiscountPercentage(SponsorshipTier? sponsorRank)
        {
            return sponsorRank switch
            {
                SponsorshipTier.Bronze => 5,    
                SponsorshipTier.Silver => 10,   
                SponsorshipTier.Gold => 15,    
                _ => 0                         
            };
        }
    }

}
