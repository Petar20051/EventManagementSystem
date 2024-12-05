using EventManagementSystem.Core;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Payments;
using EventManagementSystem.Infrastructure.Data.Enums;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.Extensions.Options;
using Moq;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ServicesTests
{
    public class StripePaymentServiceTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IDiscountService> _mockDiscountService;
        private readonly StripeSettings _stripeSettings;
        private readonly StripePaymentService _stripePaymentService;

        public StripePaymentServiceTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockDiscountService = new Mock<IDiscountService>();

            _stripeSettings = new StripeSettings
            {
                SecretKey = "test_secret_key"
            };

            var stripeOptions = Options.Create(_stripeSettings);

            _stripePaymentService = new StripePaymentService(
                _mockUserService.Object,
                stripeOptions,
                _mockDiscountService.Object
            );
        }

       

        [Fact]
        public async Task ProcessPaymentAsync_ThrowsException_WhenCustomerIdNotFound()
        {
            // Arrange
            var userId = "user1";
            var amount = 100m;
            var paymentMethodId = "pm_test";

            _mockUserService
                .Setup(us => us.GetStripeCustomerIdAsync(userId))
                .ReturnsAsync((string)null); // Simulate no customer ID

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _stripePaymentService.ProcessPaymentAsync(amount, paymentMethodId, userId));
        }

       

        [Fact]
        public async Task GetStoredCardsAsync_ReturnsEmptyList_WhenCustomerIdIsNull()
        {
            // Arrange
            var userId = "user1";

            _mockUserService
                .Setup(us => us.GetStripeCustomerIdAsync(userId))
                .ReturnsAsync((string)null); // Simulate no customer ID

            // Act
            var cards = await _stripePaymentService.GetStoredCardsAsync(userId);

            // Assert
            Assert.NotNull(cards);
            Assert.Empty(cards);
        }

       
    }

}

