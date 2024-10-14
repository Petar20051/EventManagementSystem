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
        public int ReservationId { get; set; }
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; }

        [ForeignKey(nameof(ReservationId))]
        public Reservation Reservation { get; set; }

        public int CreditCardDetailsId { get; set; }
    }
}