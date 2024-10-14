using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Models.Payments
{
    public class PaymentMethodViewModel
    {
        [Required]
        [CreditCard]
        public string CardNumber { get; set; }

        [Required]
        public long ExpirationMonth { get; set; }

        [Required]
        public long ExpirationYear { get; set; }

        [Required]
        public string CVV { get; set; }
    }
}
