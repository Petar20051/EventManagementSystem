using EventManagementSystem.Core;
using EventManagementSystem.Core.Contracts;
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
        private readonly Mock<PaymentMethodService> _mockPaymentMethodService;
        private readonly Mock<CustomerService> _mockCustomerService;
        private readonly Mock<PaymentIntentService> _mockPaymentIntentService;
        private readonly StripePaymentService _stripePaymentService;

        public StripePaymentServiceTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockDiscountService = new Mock<IDiscountService>();
            _mockPaymentMethodService = new Mock<PaymentMethodService>();
            _mockCustomerService = new Mock<CustomerService>();
            _mockPaymentIntentService = new Mock<PaymentIntentService>();

            var stripeSettings = new StripeSettings { SecretKey = "sk_test_4eC39HqLyjWDarjtT1zdp7dc" };
            var mockStripeOptions = Mock.Of<IOptions<StripeSettings>>(x => x.Value == stripeSettings);

            _stripePaymentService = new StripePaymentService(
                _mockUserService.Object,
                mockStripeOptions,
                _mockDiscountService.Object,
                _mockPaymentMethodService.Object,
                _mockCustomerService.Object,
                _mockPaymentIntentService.Object
            );
        }

        [Fact]
        public async Task CreateStripeCustomerAsync_ShouldReturnCustomerId_WhenCustomerIsCreated()
        {
            // Arrange
            var email = "test@example.com";
            var userName = "Test User";
            var expectedCustomerId = "cus_123456";

            _mockCustomerService
                .Setup(cs => cs.CreateAsync(It.IsAny<CustomerCreateOptions>(), null, default))
                .ReturnsAsync(new Customer { Id = expectedCustomerId });

            // Act
            var actualCustomerId = await _stripePaymentService.CreateStripeCustomerAsync("user-id", email, userName);

            // Assert
            Assert.Equal(expectedCustomerId, actualCustomerId);
        }

        [Fact]
        public async Task ProcessPaymentAsync_ShouldReturnStatus_WhenPaymentIsProcessed()
        {
            // Arrange
            var userId = "test-user-id";
            var paymentMethodId = "pm_123456";
            var amount = 100.00m;

            var expectedCustomerId = "cus_123456";
            var expectedStatus = "succeeded";
            var expectedDiscountedAmount = 90.00m;

            _mockUserService.Setup(us => us.GetStripeCustomerIdAsync(userId))
                            .ReturnsAsync(expectedCustomerId);

            _mockUserService.Setup(us => us.GetUserByIdAsync(userId))
                            .ReturnsAsync(new ApplicationUser { SponsorshipTier = SponsorshipTier.Silver });

            _mockDiscountService.Setup(ds => ds.ApplyDiscountAsync(SponsorshipTier.Silver, amount))
                                 .ReturnsAsync(expectedDiscountedAmount);

            _mockPaymentIntentService
                .Setup(pis => pis.CreateAsync(It.IsAny<PaymentIntentCreateOptions>(), null, default))
                .ReturnsAsync(new PaymentIntent { Status = expectedStatus });

            // Act
            var actualStatus = await _stripePaymentService.ProcessPaymentAsync(amount, paymentMethodId, userId);

            // Assert
            Assert.Equal(expectedStatus, actualStatus);
        }

        [Fact]
        public async Task AttachPaymentMethodAsync_ShouldAttachPaymentMethodToCustomer()
        {
            // Arrange
            var customerId = "cus_123456";
            var paymentMethodId = "pm_123456";

            _mockPaymentMethodService
                .Setup(pms => pms.AttachAsync(paymentMethodId, It.IsAny<PaymentMethodAttachOptions>(), null, default))
                .ReturnsAsync(new PaymentMethod
                {
                    Id = paymentMethodId,
                    Customer = new Customer
                    {
                        Id = customerId
                    }
                });

            // Act
            await _stripePaymentService.AttachPaymentMethodAsync(customerId, paymentMethodId);

            // Assert
            _mockPaymentMethodService.Verify(
                pms => pms.AttachAsync(paymentMethodId, It.IsAny<PaymentMethodAttachOptions>(), null, default),
                Times.Once
            );
        }

        [Fact]
        public async Task GetStoredCardsAsync_ShouldReturnCards_WhenCardsExist()
        {
            // Arrange
            var userId = "test-user-id";
            var customerId = "cus_123456";

            _mockUserService.Setup(us => us.GetStripeCustomerIdAsync(userId))
                            .ReturnsAsync(customerId);

            _mockPaymentMethodService
                .Setup(pms => pms.ListAsync(It.IsAny<PaymentMethodListOptions>(), null, default))
                .ReturnsAsync(new StripeList<PaymentMethod>
                {
                    Data = new List<PaymentMethod>
                    {
                    new PaymentMethod
                    {
                        Id = "pm_1",
                        BillingDetails = new PaymentMethodBillingDetails { Name = "John Doe" },
                        Card = new PaymentMethodCard
                        {
                            Last4 = "4242",
                            ExpMonth = 12,
                            ExpYear = 2024
                        }
                    }
                    }
                });

            // Act
            var cards = await _stripePaymentService.GetStoredCardsAsync(userId);

            // Assert
            Assert.Single(cards);
            Assert.Equal("pm_1", cards[0].CardId);
            Assert.Equal("John Doe", cards[0].CardHolderName);
            Assert.Equal("4242", cards[0].Last4Digits);
            Assert.Equal("12/2024", cards[0].ExpirationDate);
        }
    }
}
