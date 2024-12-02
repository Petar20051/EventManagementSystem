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

            // Set up a mock user
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
            // Act
            var result = _controller.AddPaymentMethod();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("test_publishable_key", viewResult.ViewData["PublishableKey"]);
        }

        [Fact]
        public async Task AddPaymentMethod_Post_ReturnsOkResult_WhenPaymentMethodIsValid()
        {
            // Arrange
            var inputModel = new PaymentMethodInputModel { PaymentMethodId = "pm_test" };
            _userServiceMock.Setup(x => x.GetStripeCustomerIdAsync(It.IsAny<string>())).ReturnsAsync("test_customer_id");
            _stripePaymentServiceMock.Setup(x => x.AttachPaymentMethodAsync("test_customer_id", "pm_test")).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AddPaymentMethod(inputModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Payment method added successfully.", ((dynamic)okResult.Value).message);
        }

        [Fact]
        public async Task AddPaymentMethod_Post_ReturnsBadRequest_WhenPaymentMethodIsInvalid()
        {
            // Act
            var result = await _controller.AddPaymentMethod(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid Payment Method.", ((dynamic)badRequestResult.Value).error);
        }

        [Fact]
        public async Task AddPaymentMethod_Post_ReturnsUnauthorized_WhenUserNotFound()
        {
            // Arrange
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(); // No user

            // Act
            var result = await _controller.AddPaymentMethod(new PaymentMethodInputModel { PaymentMethodId = "pm_test" });

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User not found.", ((dynamic)unauthorizedResult.Value).error);
        }

        [Fact]
        public async Task ManagePaymentMethods_ReturnsViewResult_WithPaymentMethods()
        {
            // Arrange
            var storedCards = new List<CardViewModel> { new CardViewModel { CardId = "card_1" } };
            _stripePaymentServiceMock.Setup(x => x.GetStoredCardsAsync("test_user_id")).ReturnsAsync(storedCards);

            // Act
            var result = await _controller.ManagePaymentMethods();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(storedCards, viewResult.Model);
        }

        [Fact]
        public async Task DeletePaymentMethod_ReturnsRedirectToActionResult()
        {
            // Arrange
            _paymentServiceMock.Setup(x => x.DeleteCreditCardAsync("card_1", "test_user_id")).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeletePaymentMethod("card_1");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ManagePaymentMethods", redirectResult.ActionName);
        }

        [Fact]
        public async Task DeletePaymentMethod_ReturnsUnauthorized_WhenUserNotFound()
        {
            // Arrange
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(); // No user

            // Act
            var result = await _controller.DeletePaymentMethod("card_1");

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
        }
    }
}
