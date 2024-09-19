using EventManagementSystem.Infrastructure.Constants;
using EventManagementSystem.Infrastructure.Data.Entities;
using EventManagementSystem.Infrastructure.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagementSystem.Infrastructure.Entities
{
    public class Payment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public string UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public PaymentType PaymentType { get; set; }  

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public PaymentFor PaymentFor { get; set; }  
        public CreditCardDetails CreditCardDetails { get; set; }
        public PayPalDetails PayPalDetails { get; set; }
    }
}