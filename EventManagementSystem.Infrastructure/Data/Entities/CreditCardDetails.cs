using EventManagementSystem.Infrastructure.Constants;
using System.ComponentModel.DataAnnotations;

namespace EventManagementSystem.Infrastructure.Data.Entities
{
    public class CreditCardDetails
    {
        public int Id { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        [StringLength(ValidationConstants.CardNumberLength, ErrorMessage = ValidationConstants.InvalidCardNumber)]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        [StringLength(ValidationConstants.ExpiryDateLength, ErrorMessage = ValidationConstants.InvalidExpiryDate)]
        public string ExpiryDate { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        [MaxLength(ValidationConstants.NameMaxLength, ErrorMessage = ValidationConstants.MaxLengthError)]
        public string CardHolderName { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        [StringLength(ValidationConstants.CVVLength, ErrorMessage = ValidationConstants.InvalidCVV)]
        public string CVV { get; set; }
    }
}