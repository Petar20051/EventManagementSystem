using EventManagementSystem.Core.Models.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Models.Sponsorship
{
    public class ProcessSponsorshipViewModel
    {
        public decimal Amount { get; set; }
        public string SelectedCardId { get; set; }
        public List<CardViewModel> Cards { get; set; }
    }
}
