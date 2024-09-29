using EventManagementSystem.Infrastructure.Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Models
{
    public class PaymentViewModel
    {
        public string UserId { get; set; }

        [Required(ErrorMessage = "Reservation ID is required.")]
        public int ReservationId { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment For is required.")]
        public PaymentFor PaymentFor { get; set; } // You can use the PaymentFor enum

        // Additional properties can be added as needed for your payment processing
    }
}
