using EventManagementSystem.Infrastructure.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagementSystem.Infrastructure.Entities
{
    public class Reservation
    {
        public int Id { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public int EventId { get; set; }
        [ForeignKey(nameof(EventId))]
        public Event Event { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public string UserId { get; set; }
        [ForeignKey(nameof (UserId))]
        public ApplicationUser User { get; set; }

        [Required(ErrorMessage = ValidationConstants.RequiredField)]
        public DateTime ReservationDate { get; set; }
    }
}