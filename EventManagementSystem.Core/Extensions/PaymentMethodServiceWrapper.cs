using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Extensions
{
    public class PaymentMethodServiceWrapper : IPaymentMethodServiceWrapper
    {
        private readonly IPaymentMethodService _paymentMethodService;

        public PaymentMethodServiceWrapper(IPaymentMethodService paymentMethodService)
        {
            _paymentMethodService = paymentMethodService;
        }

        public async Task<PaymentMethod> DetachAsync(string paymentMethodId, PaymentMethodDetachOptions options = null, RequestOptions requestOptions = null)
        {
            options ??= new PaymentMethodDetachOptions();
            requestOptions ??= new RequestOptions();
            return await _paymentMethodService.DetachAsync(paymentMethodId, options, requestOptions);
        }

        public async Task<IEnumerable<PaymentMethod>> GetPaymentMethodsAsync(string customerId)
        {
            var options = new PaymentMethodListOptions
            {
                Customer = customerId,
                Type = "card"
            };

            var paymentMethods = await _paymentMethodService.ListAsync(options, null);
            return paymentMethods.Data;
        }
    }
    }
