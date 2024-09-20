using EventManagementSystem.Infrastructure.Constants;
using System.ComponentModel.DataAnnotations;

namespace EventManagementSystem.Infrastructure.Entities
{
    public class Venue
    {
        public int Id { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        [MaxLength(ValidationConstants.NameMaxLength, ErrorMessage = ValidationConstants.MaxLengthError)]
        public string Name { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        [MaxLength(ValidationConstants.AddressMaxLength, ErrorMessage = ValidationConstants.MaxLengthError)]
        public string Address { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public int Capacity { get; set; }

        public ICollection<Event> Events { get; set; } 
    }
}