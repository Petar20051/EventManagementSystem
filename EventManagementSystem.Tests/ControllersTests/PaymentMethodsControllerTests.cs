using EventMaganementSystem.Controllers;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Payments;
using EventManagementSystem.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ControllersTests
{
    public class PaymentMethodsControllerTests
    {
        private readonly Mock<IPaymentService> _paymentServiceMock;
        private readonly Mock<IStripePaymentService> _stripePaymentServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IOptions<StripeSettings>> _stripeOptionsMock;
        private readonly PaymentMethodsController _controller;

        public PaymentMethodsControllerTests()
        {
            _paymentServiceMock = new Mock<IPaymentService>();
            _stripePaymentServiceMock = new Mock<IStripePaymentService>();
            _userServiceMock = new Mock<IUserService>();
            _stripeOptionsMock = new Mock<IOptions<StripeSettings>>();

            var stripeSettings = new StripeSettings { PublishableKey = "test_publishable_key" };
            _stripeOptionsMock.Setup(x => x.Value).Returns(stripeSettings);

            _controller = new PaymentMethodsController(
                _paymentServiceMock.Object,
                _stripePaymentServiceMock.Object,
                _userServiceMock.Object,
                _stripeOptionsMock.Object);

            
            var userClaims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "test_user_id") };
            var identity = new ClaimsIdentity(userClaims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public void AddPaymentMethod_Get_ReturnsViewResult()
        {
            
            var result = _controller.AddPaymentMethod();

            
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("test_publishable_key", viewResult.ViewData["PublishableKey"]);
        }

        

        [Fact]
        public async Task ManagePaymentMethods_ReturnsViewResult_WithPaymentMethods()
        {
            
            var storedCards = new List<CardViewModel> { new CardViewModel { CardId = "card_1" } };
            _stripePaymentServiceMock.Setup(x => x.GetStoredCardsAsync("test_user_id")).ReturnsAsync(storedCards);

            
            var result = await _controller.ManagePaymentMethods();

            
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(storedCards, viewResult.Model);
        }

        

        [Fact]
        public async Task DeletePaymentMethod_ReturnsUnauthorized_WhenUserNotFound()
        {
            
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(); // No user

            
            var result = await _controller.DeletePaymentMethod("card_1");

            
            var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
        }
    }
}
