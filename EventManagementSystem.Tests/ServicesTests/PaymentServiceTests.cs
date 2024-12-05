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
            // Add users
            dbContext.Users.Add(new ApplicationUser { Id = "user1", UserName = "Test User 1" });

            // Add payments with required fields
            dbContext.Payments.Add(new Payment
            {
                Id = 1,
                UserId = "user1",
                Amount = 100.0m,
                PaymentDate = new DateTime(2024, 12, 1, 12, 0, 0),
                PaymentMethod = "CreditCard",
                Status = "Completed"
            });

            // Add credit card details with required fields
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

            // Act
            await service.RecordPaymentAsync(payment);

            // Assert
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

            // Act
            var paymentMethods = await service.GetUserPaymentMethodsAsync("user1");

            // Assert
            Assert.Single(paymentMethods);
            var card = paymentMethods.First();
            Assert.Equal(1, card.Id);
            Assert.Equal("5678", card.Last4Digits);
            Assert.Equal("12/2024", card.ExpirationDate);
        }

        [Fact]
        public async Task DeleteCreditCardAsync_DetachesPaymentMethod()
        {
            var userId = "user1";
            var cardId = "card1";

            _mockUserService.Setup(s => s.GetStripeCustomerIdAsync(userId))
                .ReturnsAsync("test_customer_id");

            _mockPaymentMethodServiceWrapper.Setup(s => s.DetachAsync(cardId, null, null))
                .ReturnsAsync(new PaymentMethod { Id = cardId });

            using var dbContext = new EventDbContext(_options);
            var service = CreateService(dbContext);

            // Act
            await service.DeleteCreditCardAsync(cardId, userId);

            // Assert
            _mockUserService.Verify(s => s.GetStripeCustomerIdAsync(userId), Times.Once);
            _mockPaymentMethodServiceWrapper.Verify(s => s.DetachAsync(cardId, null, null), Times.Once);
        }

        [Fact]
        public async Task DeleteCreditCardAsync_ThrowsIfCustomerIdNotFound()
        {
            var userId = "user1";
            var cardId = "card1";

            _mockUserService.Setup(s => s.GetStripeCustomerIdAsync(userId))
                .ReturnsAsync((string)null);

            using var dbContext = new EventDbContext(_options);
            var service = CreateService(dbContext);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteCreditCardAsync(cardId, userId));
        }

        [Fact]
        public async Task GetPaymentByIdAsync_ReturnsCorrectPaymentDetails()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);
            var service = CreateService(dbContext);

            var paymentDate = new DateTime(2024, 12, 1, 12, 0, 0);

            // Act
            var paymentDetails = await service.GetPaymentByIdAsync(paymentDate);

            // Assert
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

            // Act
            var paymentDetails = await service.GetPaymentByIdAsync(nonExistentDate);

            // Assert
            Assert.Null(paymentDetails);
        }
    }
}
