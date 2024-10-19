using EventManagementSystem.Core.Models.Payments;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Models.Sponsorship
{
    public class ProcessSponsorshipViewModel
    {
        [Required(ErrorMessage = "Please enter a sponsorship amount.")]
        [Range(1, double.MaxValue, ErrorMessage = "The amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Please select a payment method.")]
        public string SelectedCardId { get; set; }

        public List<CardViewModel> Cards { get; set; } = new List<CardViewModel>();

        [Required]
        public int EventId { get; set; }
    }
}
