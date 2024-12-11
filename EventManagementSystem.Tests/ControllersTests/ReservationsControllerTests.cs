using EventMaganementSystem.Controllers;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Reservation;
using EventManagementSystem.Infrastructure.Data.Enums;
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
    public class ReservationsControllerTests
    {
        private readonly Mock<IReservationService> _mockReservationService;
        private readonly Mock<IEventService> _mockEventService;
        private readonly Mock<IStripePaymentService> _mockStripePaymentService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IDiscountService> _mockDiscountService;
        private readonly Mock<IProfileService> _mockProfileService;

        public ReservationsControllerTests()
        {
            _mockReservationService = new Mock<IReservationService>();
            _mockEventService = new Mock<IEventService>();
            _mockStripePaymentService = new Mock<IStripePaymentService>();
            _mockUserService = new Mock<IUserService>();
            _mockDiscountService = new Mock<IDiscountService>();
            _mockProfileService = new Mock<IProfileService>();
        }

        private ClaimsPrincipal CreateUserPrincipal(string userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuthentication"));
        }

        [Fact]
        public async Task Create_GET_RedirectsToLogin_WhenUserIsNotAuthenticated()
        {
            
            var controller = new ReservationsController(
                _mockReservationService.Object,
                _mockEventService.Object,
                _mockStripePaymentService.Object,
                _mockUserService.Object,
                _mockDiscountService.Object,
                _mockProfileService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            
            var result = await controller.Create();

            
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal("/Identity/Account/Login", redirectResult.Url);
        }

        [Fact]
        public async Task Create_GET_ReturnsViewResult_WithReservationViewModel()
        {
            
            var userId = "user1";
            var events = new List<Event>
        {
            new Event { Id = 1, Name = "Event 1" },
            new Event { Id = 2, Name = "Event 2" }
        };

            _mockEventService.Setup(s => s.GetAllEventsAsync()).ReturnsAsync(events);

            var controller = new ReservationsController(
                _mockReservationService.Object,
                _mockEventService.Object,
                _mockStripePaymentService.Object,
                _mockUserService.Object,
                _mockDiscountService.Object,
                _mockProfileService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateUserPrincipal(userId)
                    }
                }
            };

            
            var result = await controller.Create();

            
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ReservationViewModel>(viewResult.Model);
            Assert.Equal(2, model.Events.Count());
        }

        [Fact]
        public async Task Create_POST_RedirectsToIndex_WhenModelStateIsValid()
        {
            
            var userId = "user1";
            var viewModel = new ReservationViewModel
            {
                EventId = 1,
                AttendeesCount = 2
            };

            var currentEvent = new Event
            {
                Id = 1,
                TicketPrice = 50
            };

            _mockEventService.Setup(s => s.GetEventByIdAsync(1)).ReturnsAsync(currentEvent);

            var controller = new ReservationsController(
                _mockReservationService.Object,
                _mockEventService.Object,
                _mockStripePaymentService.Object,
                _mockUserService.Object,
                _mockDiscountService.Object,
                _mockProfileService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateUserPrincipal(userId)
                    }
                }
            };

            
            var result = await controller.Create(viewModel);

            
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _mockReservationService.Verify(s => s.CreateReservationAsync(It.IsAny<Reservation>()), Times.Once);
        }

        [Fact]
        public async Task Create_POST_ReturnsViewResult_WhenModelStateIsInvalid()
        {
            
            var userId = "user1";
            var viewModel = new ReservationViewModel();

            var controller = new ReservationsController(
                _mockReservationService.Object,
                _mockEventService.Object,
                _mockStripePaymentService.Object,
                _mockUserService.Object,
                _mockDiscountService.Object,
                _mockProfileService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateUserPrincipal(userId)
                    }
                }
            };

            controller.ModelState.AddModelError("Error", "Invalid model");

            
            var result = await controller.Create(viewModel);

            
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(viewModel, viewResult.Model);
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithReservations()
        {
            
            var userId = "user1";
            var reservations = new List<Reservation>
    {
        new Reservation
        {
            Id = 1,
            UserId = userId,
            Event = new Event { Id = 1, Name = "Event 1", TicketPrice = 50 },
            AttendeesCount = 2,
            ReservationDate = DateTime.UtcNow,
            IsPaid = true
        }
    };

            _mockReservationService.Setup(s => s.GetAllReservationsAsync()).ReturnsAsync(reservations);
            _mockUserService.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(new ApplicationUser
            {
                Id = userId,
                SponsorshipTier = SponsorshipTier.Bronze
            });
            _mockDiscountService
                .Setup(s => s.ApplyDiscountAsync(SponsorshipTier.Bronze, It.IsAny<decimal>()))
                .ReturnsAsync((SponsorshipTier tier, decimal amount) => amount * 0.9m); 

            var controller = new ReservationsController(
                _mockReservationService.Object,
                _mockEventService.Object,
                _mockStripePaymentService.Object,
                _mockUserService.Object,
                _mockDiscountService.Object,
                _mockProfileService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateUserPrincipal(userId)
                    }
                }
            };

            
            var result = await controller.Index();

            
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<ReservationViewModel>>(viewResult.Model);

            Assert.Single(model);
            Assert.Equal("Event 1", model[0].EventName);
            Assert.Equal(90, model[0].DiscountedAmount); 
            Assert.Equal(100, model[0].TotalAmount);
        }

    }
}
