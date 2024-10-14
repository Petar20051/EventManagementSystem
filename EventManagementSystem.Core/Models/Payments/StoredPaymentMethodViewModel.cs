using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Models.Payments
{
    public class StoredPaymentMethodViewModel
    {
        public int Id { get; set; }
        public string? CardHolderName { get; set; }
        public string Last4Digits { get; set; }
        public string ExpirationDate { get; set; }
    }
}
