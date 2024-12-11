using EventManagementSystem.Core.Models.Payments;
using EventManagementSystem.Infrastructure.Data.Enums;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Contracts
{
    public interface IStripePaymentService
    {
        Task<string> ProcessPaymentAsync(decimal amount, string paymentMethodId, string userId);
        Task<string> ProcessSponsorshipPaymentAsync(decimal amount, string paymentMethodId, string userId);
        Task<List<CardViewModel>> GetStoredCardsAsync(string userId);
        
        Task<string> CreateStripeCustomerAsync(string userId,string email, string userName);
        
        Task AttachPaymentMethodAsync(string customerId, string paymentMethodId);

        Task<PaymentMethod> GetPaymentMethodAsync(string paymentMethodId);
        Task UpdateCustomerDefaultPaymentMethodAsync(string customerId, string paymentMethodId);
    }
}
