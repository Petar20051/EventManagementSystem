using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Extensions;
using EventManagementSystem.Core.Models.Payments;
using EventManagementSystem.Core.Services;
using EventManagementSystem.Infrastructure.Data.Entities;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;

using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ServicesTests
{
   
public class PaymentServiceTests
    {
        private readonly DbContextOptions<EventDbContext> _options;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IPaymentMethodServiceWrapper> _mockPaymentMethodServiceWrapper;

        public PaymentServiceTests()
        {
            _options = new DbContextOptionsBuilder<EventDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _mockUserService = new Mock<IUserService>();
            _mockPaymentMethodServiceWrapper = new Mock<IPaymentMethodServiceWrapper>();
        }

        private PaymentService CreateService(EventDbContext dbContext)
        {
            return new PaymentService(dbContext, _mockUserService.Object, _mockPaymentMethodServiceWrapper.Object);
        }

        private async Task SeedDataAsync(EventDbContext dbContext)
        {
            
            dbContext.Users.Add(new ApplicationUser { Id = "user1", UserName = "Test User 1" });

            
            dbContext.Payments.Add(new Payment
            {
                Id = 1,
                UserId = "user1",
                Amount = 100.0m,
                PaymentDate = new DateTime(2024, 12, 1, 12, 0, 0),
                PaymentMethod = "CreditCard",
                Status = "Completed"
            });

            
            dbContext.CreditCardDetails.Add(new CreditCardDetails
            {
                Id = 1,
                UserId = "user1",
                CardNumber = "1234567812345678",
                ExpirationMonth = "12",
                ExpirationYear = "2024",
                CardBrand = "Visa",
                PaymentMethodId = "pm_test"
            });

            await dbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task RecordPaymentAsync_AddsPaymentToDatabase()
        {
            using var dbContext = new EventDbContext(_options);
            var service = CreateService(dbContext);

            var payment = new Payment
            {
                UserId = "user1",
                Amount = 200.0m,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = "PayPal",
                Status = "Pending"
            };

            
            await service.RecordPaymentAsync(payment);

            
            var savedPayment = await dbContext.Payments.FirstOrDefaultAsync(p => p.Amount == 200.0m);
            Assert.NotNull(savedPayment);
            Assert.Equal("PayPal", savedPayment.PaymentMethod);
        }

        [Fact]
        public async Task GetUserPaymentMethodsAsync_ReturnsStoredPaymentMethods()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);
            var service = CreateService(dbContext);

            
            var paymentMethods = await service.GetUserPaymentMethodsAsync("user1");

            
            Assert.Single(paymentMethods);
            var card = paymentMethods.First();
            Assert.Equal(1, card.Id);
            Assert.Equal("5678", card.Last4Digits);
            Assert.Equal("12/2024", card.ExpirationDate);
        }

        [Fact]
        public async Task DeleteCreditCardAsync_ThrowsIfCardIdIsNullOrEmpty()
        {
            
            var userId = "user1";
            string cardId = null; 

            using var dbContext = new EventDbContext(_options);
            var service = CreateService(dbContext);

            
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => service.DeleteCreditCardAsync(cardId, userId));
            Assert.Equal("Card ID cannot be null or empty. (Parameter 'cardId')", exception.Message);
        }



        [Fact]
        public async Task DeleteCreditCardAsync_ThrowsIfStripeExceptionOccurs()
        {
            
            var userId = "user1";
            var cardId = "card1";

            _mockPaymentMethodServiceWrapper.Setup(s => s.DetachAsync(cardId,null,null))
      .ThrowsAsync(new StripeException("Stripe error occurred"));


            using var dbContext = new EventDbContext(_options);
            var service = CreateService(dbContext);

            
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteCreditCardAsync(cardId, userId));
            Assert.Contains("An error occurred while detaching the payment method: Stripe error occurred", exception.Message);
            _mockPaymentMethodServiceWrapper.Verify(s => s.DetachAsync(cardId, null, null), Times.Once);
        }


        [Fact]
        public async Task DeleteCreditCardAsync_ThrowsIfUnexpectedExceptionOccurs()
        {
            
            var userId = "user1";
            var cardId = "card1";

            _mockPaymentMethodServiceWrapper.Setup(s => s.DetachAsync(cardId, null, null))
                .ThrowsAsync(new Exception("Unexpected exception"));

            using var dbContext = new EventDbContext(_options);
            var service = CreateService(dbContext);

            
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteCreditCardAsync(cardId, userId));
            Assert.Contains("An unexpected error occurred while deleting the payment method.", exception.Message);
            _mockPaymentMethodServiceWrapper.Verify(s => s.DetachAsync(cardId, null, null), Times.Once);
        }


        [Fact]
        public async Task DeleteCreditCardAsync_SuccessfullyDeletesCreditCard()
        {
            
            var userId = "user1";
            var cardId = "card1";

            _mockPaymentMethodServiceWrapper.Setup(s => s.DetachAsync(cardId, null, null))
                .ReturnsAsync(new PaymentMethod { Id = cardId }); 

            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);
            var service = CreateService(dbContext);

            
            await service.DeleteCreditCardAsync(cardId, userId);

            
            _mockPaymentMethodServiceWrapper.Verify(s => s.DetachAsync(cardId, null, null), Times.Once);

            var remainingCards = await dbContext.CreditCardDetails.Where(c => c.UserId == userId).ToListAsync();
            Assert.DoesNotContain(remainingCards, c => c.PaymentMethodId == cardId);
        }



        [Fact]
        public async Task GetPaymentByIdAsync_ReturnsCorrectPaymentDetails()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);
            var service = CreateService(dbContext);

            var paymentDate = new DateTime(2024, 12, 1, 12, 0, 0);

            
            var paymentDetails = await service.GetPaymentByIdAsync(paymentDate);

            
            Assert.NotNull(paymentDetails);
            Assert.Equal(1, paymentDetails.PaymentId);
            Assert.Equal(100.0m, paymentDetails.Amount);
            Assert.Equal("CreditCard", paymentDetails.PaymentMethod);
            Assert.Equal("Completed", paymentDetails.Status);
        }

        [Fact]
        public async Task GetPaymentByIdAsync_ReturnsNullIfNoPaymentFound()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);
            var service = CreateService(dbContext);

            var nonExistentDate = new DateTime(2023, 1, 1, 12, 0, 0);

            
            var paymentDetails = await service.GetPaymentByIdAsync(nonExistentDate);

            
            Assert.Null(paymentDetails);
        }
    }
}
