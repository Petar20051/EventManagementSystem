using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
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

        public PaymentService(EventDbContext context,IUserService userService)
        {
            _context = context;
            _userService = userService;
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

        // Implementing DeleteCreditCardAsync
        public async Task DeleteCreditCardAsync(string cardId, string userId)
        {
            var customerId = await _userService.GetStripeCustomerIdAsync(userId);
            if (string.IsNullOrEmpty(customerId))
            {
                throw new InvalidOperationException("Customer ID not found.");
            }

            var service = new PaymentMethodService();

            // Detach the card from the customer
            await service.DetachAsync(cardId);
        }
    }
}
