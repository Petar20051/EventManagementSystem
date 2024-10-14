using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Models.Payments
{
    public class ProcessPaymentViewModel
    {
        public int ReservationId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethodID { get; set; }
        public string SelectedCardId { get; set; }
        public IEnumerable<CardViewModel> StoredCards { get; set; }
    }
}
