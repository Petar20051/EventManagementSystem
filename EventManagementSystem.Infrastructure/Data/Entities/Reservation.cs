using EventManagementSystem.Infrastructure.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagementSystem.Infrastructure.Entities
{
    public class Reservation
    {
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }

        [ForeignKey(nameof(EventId))]
        public Event Event { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ReservationDate { get; set; } = DateTime.Now;

        public bool IsPaid { get; set; } = false; // Default to unpaid until payment is confirmed

        [DataType(DataType.DateTime)]
        public DateTime? PaymentDate { get; set; }
    }
}