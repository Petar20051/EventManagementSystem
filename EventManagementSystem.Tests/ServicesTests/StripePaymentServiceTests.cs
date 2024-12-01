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
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IDiscountService> _discountServiceMock;
        private readonly Mock<IOptions<StripeSettings>> _stripeOptionsMock;
        private readonly StripePaymentService _stripePaymentService;

        public StripePaymentServiceTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _discountServiceMock = new Mock<IDiscountService>();
            _stripeOptionsMock = new Mock<IOptions<StripeSettings>>();

            // Mock Stripe settings
            _stripeOptionsMock.Setup(o => o.Value).Returns(new StripeSettings { SecretKey = "sk_test_secretkey" });

            _stripePaymentService = new StripePaymentService(
                _userServiceMock.Object,
                _stripeOptionsMock.Object,
                _discountServiceMock.Object
            );
        }

        [Fact]
        public async Task GetStoredCardsAsync_ShouldReturnEmptyList_WhenNoCustomerId()
        {
            // Arrange
            var userId = "user_id";
            _userServiceMock.Setup(us => us.GetStripeCustomerIdAsync(userId)).ReturnsAsync((string)null);

            // Act
            var result = await _stripePaymentService.GetStoredCardsAsync(userId);

            // Assert
            Assert.Empty(result);
        }

    }
}
