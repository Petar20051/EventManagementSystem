using EventManagementSystem.Infrastructure.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Contracts
{
    public interface IDiscountService
    {
        Task<decimal> ApplyDiscountAsync(SponsorshipTier? sponsorRank, decimal originalPrice);
    }
}
