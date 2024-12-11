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
        public string UserId { get; set; }  

        [Required]
        public string PaymentMethodId { get; set; } 

        public string CardBrand { get; set; } 
        public string CardNumber { get; set; } 

        public string ExpirationMonth { get; set; }
        public string ExpirationYear { get; set; }

        public bool IsDefault { get; set; }
    }
}