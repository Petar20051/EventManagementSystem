using EventManagementSystem.Infrastructure.Constants;
using System.ComponentModel.DataAnnotations;

namespace EventManagementSystem.Infrastructure.Entities
{
    public class PayPalDetails
    {
        public int Id { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        [EmailAddress(ErrorMessage = ValidationConstants.InvalidEmail)]
        public string PayPalEmail { get; set; }
    }
}