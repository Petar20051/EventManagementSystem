using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Extensions
{
    public class PaymentMethodServiceWrapper:IPaymentMethodServiceWrapper
    {
        private readonly PaymentMethodService _paymentMethodService;

        public PaymentMethodServiceWrapper()
        {
            _paymentMethodService = new PaymentMethodService();
        }

        public async Task<PaymentMethod> DetachAsync(string paymentMethodId, PaymentMethodDetachOptions options = null, RequestOptions requestOptions = null)
        {
            return await _paymentMethodService.DetachAsync(paymentMethodId, options, requestOptions);
        }

        public async Task<IEnumerable<PaymentMethod>> GetPaymentMethodsAsync(string customerId)
        {
            var options = new PaymentMethodListOptions
            {
                Customer = customerId,
                Type = "card" // Filter for card payment methods
            };

            var paymentMethods = await _paymentMethodService.ListAsync(options);
            return paymentMethods.Data;
        }
    }

}
