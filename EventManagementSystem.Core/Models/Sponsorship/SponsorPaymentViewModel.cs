using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Models.Sponsorship
{
    public class SponsorPaymentViewModel
    {
        public int EventId { get; set; }

        [Required(ErrorMessage = "Please enter the sponsorship amount.")]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        // ID of the selected saved card (if the user has any)
        public string? SelectedCardId { get; set; }

        // Optionally, display saved cards (if the user has any)
        public List<string> SavedCards { get; set; } = new List<string>();

        // For entering new card details
        public string CardNumber { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public string Cvc { get; set; }
    }

}
