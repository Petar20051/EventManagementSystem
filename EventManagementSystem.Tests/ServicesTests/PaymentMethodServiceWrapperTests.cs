using EventManagementSystem.Core.Extensions;
using Moq;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ServicesTests
{
    public class PaymentMethodServiceWrapperTests
    {
        private readonly Mock<IPaymentMethodService> _mockPaymentMethodService;
        private readonly PaymentMethodServiceWrapper _serviceWrapper;

        public PaymentMethodServiceWrapperTests()
        {
            _mockPaymentMethodService = new Mock<IPaymentMethodService>();
            _serviceWrapper = new PaymentMethodServiceWrapper(_mockPaymentMethodService.Object);
        }

        [Fact]
        public async Task DetachAsync_CallsServiceAndReturnsDetachedPaymentMethod()
        {
            
            var paymentMethodId = "pm_test";
            var expectedPaymentMethod = new PaymentMethod { Id = paymentMethodId };

            _mockPaymentMethodService
                .Setup(s => s.DetachAsync(
                    It.Is<string>(id => id == paymentMethodId),
                    It.IsAny<PaymentMethodDetachOptions>(),
                    It.IsAny<RequestOptions>()))
                .ReturnsAsync(expectedPaymentMethod);

            
            var result = await _serviceWrapper.DetachAsync(paymentMethodId);

            
            Assert.NotNull(result);
            Assert.Equal(paymentMethodId, result.Id);

            _mockPaymentMethodService.Verify(s => s.DetachAsync(
                It.Is<string>(id => id == paymentMethodId),
                It.IsAny<PaymentMethodDetachOptions>(),
                It.IsAny<RequestOptions>()), Times.Once);
        }

        [Fact]
        public async Task GetPaymentMethodsAsync_ReturnsListOfPaymentMethods()
        {
            
            var customerId = "cus_test";
            var paymentMethods = new List<PaymentMethod>
        {
            new PaymentMethod { Id = "pm_1", Card = new PaymentMethodCard { Brand = "Visa", Last4 = "1234" } },
            new PaymentMethod { Id = "pm_2", Card = new PaymentMethodCard { Brand = "Mastercard", Last4 = "5678" } }
        };

            _mockPaymentMethodService
                .Setup(s => s.ListAsync(
                    It.Is<PaymentMethodListOptions>(o => o.Customer == customerId && o.Type == "card"),
                    It.IsAny<RequestOptions>()))
                .ReturnsAsync(new StripeList<PaymentMethod> { Data = paymentMethods });

            
            var result = await _serviceWrapper.GetPaymentMethodsAsync(customerId);

            
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, pm => pm.Id == "pm_1" && pm.Card.Brand == "Visa");
            Assert.Contains(result, pm => pm.Id == "pm_2" && pm.Card.Brand == "Mastercard");

            _mockPaymentMethodService.Verify(s => s.ListAsync(
                It.Is<PaymentMethodListOptions>(o => o.Customer == customerId && o.Type == "card"),
                It.IsAny<RequestOptions>()), Times.Once);
        }
    }
}

