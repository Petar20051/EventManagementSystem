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
            if (string.IsNullOrEmpty(paymentMethodId))
                throw new ArgumentNullException(nameof(paymentMethodId), "PaymentMethodId cannot be null or empty.");

            try
            {
                // Call Stripe API to detach the payment method
                return await _paymentMethodService.DetachAsync(paymentMethodId, options, requestOptions);
            }
            catch (StripeException ex)
            {
                // Handle Stripe-specific exceptions
                throw new InvalidOperationException($"An error occurred while detaching the payment method: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                // Handle generic exceptions
                throw new InvalidOperationException("An unexpected error occurred while detaching the payment method.", ex);
            }
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
