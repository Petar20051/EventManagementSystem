using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PayPalCheckoutSdk.Orders;
using System.Collections.Generic;

namespace EventManagementSystem.Infrastructure.Builders
{
    

    public static class PayPalOrderBuilder
    {
        public static OrderRequest BuildOrder()
        {
            var orderRequest = new OrderRequest()
            {
                CheckoutPaymentIntent = "CAPTURE",
                PurchaseUnits = new List<PurchaseUnitRequest>
            {
                new PurchaseUnitRequest
                {
                    AmountWithBreakdown = new AmountWithBreakdown
                    {
                        CurrencyCode = "USD",
                        Value = "10.00"
                    }
                }
            }
            };
            return orderRequest;
        }
    }

}
