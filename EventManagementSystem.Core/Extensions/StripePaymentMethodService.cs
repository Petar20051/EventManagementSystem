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

        // Implementation of DetachAsync
        public async Task<PaymentMethod> DetachAsync(string paymentMethodId, PaymentMethodDetachOptions options = null, RequestOptions requestOptions = null)
        {
            if (string.IsNullOrWhiteSpace(paymentMethodId))
            {
                throw new ArgumentNullException(nameof(paymentMethodId), "Payment method ID cannot be null or empty.");
            }

            options ??= new PaymentMethodDetachOptions();
            requestOptions ??= new RequestOptions();

            try
            {
                return await _stripeService.DetachAsync(paymentMethodId, options, requestOptions);
            }
            catch (StripeException ex)
            {
                throw new InvalidOperationException($"Failed to detach payment method. Stripe error: {ex.Message}", ex);
            }
        }

        // Implementation of ListAsync
        public async Task<StripeList<PaymentMethod>> ListAsync(PaymentMethodListOptions options, RequestOptions requestOptions = null)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options), "Options cannot be null.");
            }

            try
            {
                return await _stripeService.ListAsync(options, requestOptions);
            }
            catch (StripeException ex)
            {
                throw new InvalidOperationException($"Failed to list payment methods. Stripe error: {ex.Message}", ex);
            }
        }
    }
}
