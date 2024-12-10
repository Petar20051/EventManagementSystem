using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Extensions;
using EventManagementSystem.Core.Models.Payments;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Services
{
    public class PaymentService:IPaymentService
    {
        private readonly EventDbContext _context;
        private readonly IUserService _userService;
        private readonly IPaymentMethodServiceWrapper _paymentMethodServiceWrapper;

        public PaymentService(EventDbContext context,IUserService userService,IPaymentMethodServiceWrapper paymentMethodServiceWrapper)
        {
            _context = context;
            _userService = userService;
            _paymentMethodServiceWrapper = paymentMethodServiceWrapper;
        }

        public async Task RecordPaymentAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<StoredPaymentMethodViewModel>> GetUserPaymentMethodsAsync(string userId)
        {
           
            var creditCards = await _context.CreditCardDetails
                .Where(c => c.UserId == userId)
                .Select(c => new StoredPaymentMethodViewModel
                {
                    Id = c.Id,
                    Last4Digits = c.CardNumber.Substring(c.CardNumber.Length - 4),
                    ExpirationDate = $"{c.ExpirationMonth}/{c.ExpirationYear}"
                })
                .ToListAsync();

            return creditCards;
        }

        public async Task DeleteCreditCardAsync(string cardId, string userId)
        {
            if (string.IsNullOrEmpty(cardId))
                throw new ArgumentNullException(nameof(cardId), "Card ID cannot be null or empty.");

            try
            {
                // Call the StripePaymentMethodService to detach the payment method
                await _paymentMethodServiceWrapper.DetachAsync(cardId);
            }
            catch (StripeException ex)
            {
                throw new InvalidOperationException($"An error occurred while detaching the payment method: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occurred while deleting the payment method.", ex);
            }
        }

        public async Task<PaymentDetailsViewModel> GetPaymentByIdAsync(DateTime? paymentDate)
        {
            var payment = await _context.Payments
                .Where(p =>
    p.PaymentDate.Year == paymentDate.Value.Year &&
    p.PaymentDate.Month == paymentDate.Value.Month &&
    p.PaymentDate.Day == paymentDate.Value.Day &&
    p.PaymentDate.Hour == paymentDate.Value.Hour &&
    p.PaymentDate.Minute == paymentDate.Value.Minute &&
    p.PaymentDate.Second == paymentDate.Value.Second
)
                .Select(p => new PaymentDetailsViewModel
                {
                    PaymentId = p.Id,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    PaymentMethod = p.PaymentMethod,
                    Status = p.Status
                })
                .FirstOrDefaultAsync();

            return payment;
        }
    }
}
