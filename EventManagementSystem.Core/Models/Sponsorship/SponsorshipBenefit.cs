using EventManagementSystem.Infrastructure.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Models.Sponsorship
{
    public class SponsorshipBenefit
    {
        public SponsorshipTier Tier { get; set; }
        public string Description { get; set; }
        public decimal MinimumSponsorshipAmount { get; set; }
    }
}
