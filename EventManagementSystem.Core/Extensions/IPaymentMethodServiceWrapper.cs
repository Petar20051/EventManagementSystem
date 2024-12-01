using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Extensions
{
    public interface IPaymentMethodServiceWrapper
    {
        Task<PaymentMethod> DetachAsync(string paymentMethodId, PaymentMethodDetachOptions options = null, RequestOptions requestOptions = null);
        Task<IEnumerable<PaymentMethod>> GetPaymentMethodsAsync(string customerId);
    }
}
