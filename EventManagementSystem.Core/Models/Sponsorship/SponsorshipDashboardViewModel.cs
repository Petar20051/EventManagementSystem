using EventManagementSystem.Infrastructure.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Models.Sponsorship
{
    public class SponsorshipDashboardViewModel
    {
        public SponsorshipTier CurrentTier { get; set; }
        public decimal SponsoredAmount { get; set; }
        public List<SponsorshipBenefit> Benefits { get; set; } = new List<SponsorshipBenefit>();
    }
}
