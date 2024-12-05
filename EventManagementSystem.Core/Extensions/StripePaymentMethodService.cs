using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Extensions
{
    public class StripePaymentMethodService: IPaymentMethodService
    {
        private readonly PaymentMethodService _stripeService;

        public StripePaymentMethodService()
        {
            _stripeService = new PaymentMethodService();
        }

     

        public Task<PaymentMethod> DetachAsync(string paymentMethodId, PaymentMethodDetachOptions options, RequestOptions requestOptions)
        {
            throw new NotImplementedException();
        }

        public Task<StripeList<PaymentMethod>> ListAsync(PaymentMethodListOptions options, RequestOptions requestOptions)
        {
            throw new NotImplementedException();
        }
    }
}
