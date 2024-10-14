using EventManagementSystem.Infrastructure.Constants;
using EventManagementSystem.Infrastructure.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagementSystem.Infrastructure.Data.Entities
{
    public class CreditCardDetails
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }  // Foreign key to the user

        [Required]
        public string PaymentMethodId { get; set; } // Stripe's tokenized PaymentMethod ID

        public string CardBrand { get; set; } // E.g., Visa, Mastercard
        public string CardNumber { get; set; } // E.g., "4242"

        public string ExpirationMonth { get; set; }
        public string ExpirationYear { get; set; }

        public bool IsDefault { get; set; }
    }
}