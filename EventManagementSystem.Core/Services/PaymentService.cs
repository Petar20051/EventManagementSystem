using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Payments;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
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

        public PaymentService(EventDbContext context)
        {
            _context = context;
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
        public async Task DeleteCreditCardAsync(int id, string userId)
        {
            var creditCard = await _context.CreditCardDetails
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (creditCard != null)
            {
                _context.CreditCardDetails.Remove(creditCard);
                await _context.SaveChangesAsync();
            }
        }
    }
}
