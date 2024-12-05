using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Extensions
{
   public interface IPaymentMethodService
    {
        Task<PaymentMethod> DetachAsync(string paymentMethodId, PaymentMethodDetachOptions options, RequestOptions requestOptions);
        Task<StripeList<PaymentMethod>> ListAsync(PaymentMethodListOptions options, RequestOptions requestOptions);
    }
}
