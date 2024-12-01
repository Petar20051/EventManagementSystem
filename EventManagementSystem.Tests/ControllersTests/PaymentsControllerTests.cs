using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Payments;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ControllersTests
{
    public class PaymentsControllerTests
    {
        private readonly Mock<IStripePaymentService> _mockStripePaymentService;
        private readonly Mock<IReservationService> _mockReservationService;
        private readonly Mock<IPaymentService> _mockPaymentService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ITicketService> _mockTicketService;
        private readonly Mock<IUserEventService> _mockUserEventService;

        public PaymentsControllerTests()
        {
            _mockStripePaymentService = new Mock<IStripePaymentService>();
            _mockReservationService = new Mock<IReservationService>();
            _mockPaymentService = new Mock<IPaymentService>();
            _mockUserService = new Mock<IUserService>();
            _mockTicketService = new Mock<ITicketService>();
            _mockUserEventService = new Mock<IUserEventService>();


        }

        private ClaimsPrincipal CreateUserPrincipal(string userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuthentication"));
        }

        [Fact]
        public async Task ProcessPayment_GET_ReturnsNotFound_WhenReservationDoesNotExist()
        {
            // Arrange
            _mockReservationService.Setup(s => s.GetReservationByIdAsync(It.IsAny<int>())).ReturnsAsync((Reservation)null);

            var controller = new PaymentsController(
                _mockStripePaymentService.Object,
                _mockReservationService.Object,
                _mockPaymentService.Object,
                _mockUserService.Object,
                _mockTicketService.Object,
                _mockUserEventService.Object);

            // Act
            var result = await controller.ProcessPayment(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Reservation not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task ProcessPayment_GET_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            _mockReservationService.Setup(s => s.GetReservationByIdAsync(It.IsAny<int>())).ReturnsAsync(new Reservation());

            var controller = new PaymentsController(
                _mockStripePaymentService.Object,
                _mockReservationService.Object,
                _mockPaymentService.Object,
                _mockUserService.Object,
                _mockTicketService.Object,
                _mockUserEventService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = null }
                }
            };

            // Act
            var result = await controller.ProcessPayment(1);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User is not authenticated.", unauthorizedResult.Value);
        }

        [Fact]
        public async Task ProcessPayment_GET_RedirectsToAddPaymentMethod_WhenNoStoredPaymentMethodsExist()
        {
            // Arrange
            var userId = "user1";
            _mockReservationService.Setup(s => s.GetReservationByIdAsync(It.IsAny<int>())).ReturnsAsync(new Reservation { Id = 1, TotalAmount = 100 });

            // Correctly mock the stored cards as an empty list
            _mockStripePaymentService
                .Setup(s => s.GetStoredCardsAsync(userId))
                .ReturnsAsync(new List<CardViewModel>());

            var controller = new PaymentsController(
                _mockStripePaymentService.Object,
                _mockReservationService.Object,
                _mockPaymentService.Object,
                _mockUserService.Object,
                _mockTicketService.Object,
                _mockUserEventService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateUserPrincipal(userId)
                    }
                }
            };

            // Act
            var result = await controller.ProcessPayment(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AddPaymentMethod", redirectResult.ActionName);
            Assert.Equal("PaymentMethods", redirectResult.ControllerName);
        }


        [Fact]
        public async Task ProcessPayment_GET_ReturnsViewResult_WithViewModel()
        {
            // Arrange
            var userId = "user1";
            var storedCards = new List<CardViewModel>
    {
        new CardViewModel { CardId = "card_1", Last4Digits = "1234"}
    };
            var reservation = new Reservation { Id = 1, TotalAmount = 100, EventId = 1 };

            _mockReservationService.Setup(s => s.GetReservationByIdAsync(1)).ReturnsAsync(reservation);
            _mockStripePaymentService.Setup(s => s.GetStoredCardsAsync(userId)).ReturnsAsync(storedCards);

            var controller = new PaymentsController(
                _mockStripePaymentService.Object,
                _mockReservationService.Object,
                _mockPaymentService.Object,
                _mockUserService.Object,
                _mockTicketService.Object,
                _mockUserEventService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateUserPrincipal(userId)
                    }
                }
            };

            // Act
            var result = await controller.ProcessPayment(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ProcessPaymentViewModel>(viewResult.Model);

            Assert.Equal(1, model.ReservationId);
            Assert.Equal(100, model.Amount);
            Assert.Single(model.StoredCards);

            var card = model.StoredCards.First();
            Assert.Equal("card_1", card.CardId);
            Assert.Equal("1234", card.Last4Digits);
           
        }

        [Fact]
        public async Task ProcessPayment_POST_RedirectsToPaymentSuccess_WhenPaymentSucceeds()
        {
            // Arrange
            var userId = "user1";
            var reservation = new Reservation { Id = 1, TotalAmount = 100, EventId = 1, UserId = userId };
            var model = new ProcessPaymentViewModel { Amount = 100, SelectedCardId = "card_1", ReservationId = 1 };

            _mockReservationService.Setup(s => s.GetReservationByIdAsync(1)).ReturnsAsync(reservation);
            _mockStripePaymentService.Setup(s => s.ProcessPaymentAsync(model.Amount, model.SelectedCardId, userId)).ReturnsAsync("succeeded");

            var controller = new PaymentsController(
                _mockStripePaymentService.Object,
                _mockReservationService.Object,
                _mockPaymentService.Object,
                _mockUserService.Object,
                _mockTicketService.Object,
                _mockUserEventService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateUserPrincipal(userId)
                    }
                }
            };

            // Act
            var result = await controller.ProcessPayment(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("PaymentSuccess", redirectResult.ActionName);
        }

        [Fact]
        public async Task ProcessPayment_POST_RedirectsToPaymentFailed_WhenPaymentFails()
        {
            // Arrange
            var userId = "user1";
            var model = new ProcessPaymentViewModel { Amount = 100, SelectedCardId = "card_1", ReservationId = 1 };

            _mockStripePaymentService.Setup(s => s.ProcessPaymentAsync(model.Amount, model.SelectedCardId, userId)).ReturnsAsync("failed");

            var controller = new PaymentsController(
                _mockStripePaymentService.Object,
                _mockReservationService.Object,
                _mockPaymentService.Object,
                _mockUserService.Object,
                _mockTicketService.Object,
                _mockUserEventService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateUserPrincipal(userId)
                    }
                }
            };

            // Act
            var result = await controller.ProcessPayment(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("PaymentFailed", redirectResult.ActionName);
        }

        

        [Fact]
        public void PaymentSuccess_ReturnsViewResult()
        {
            // Arrange
            var controller = new PaymentsController(
                _mockStripePaymentService.Object,
                _mockReservationService.Object,
                _mockPaymentService.Object,
                _mockUserService.Object,
                _mockTicketService.Object,
                _mockUserEventService.Object);

            // Act
            var result = controller.PaymentSuccess();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Payment successful!", viewResult.ViewData["Message"]);
        }

        [Fact]
        public void PaymentFailed_ReturnsViewResult()
        {
            // Arrange
            var controller = new PaymentsController(
                _mockStripePaymentService.Object,
                _mockReservationService.Object,
                _mockPaymentService.Object,
                _mockUserService.Object,
                _mockTicketService.Object,
                _mockUserEventService.Object);

            // Act
            var result = controller.PaymentFailed();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Payment failed!", viewResult.ViewData["Message"]);
        }
    }
}
