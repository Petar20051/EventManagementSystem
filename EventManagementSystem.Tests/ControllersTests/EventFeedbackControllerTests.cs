using EventMaganementSystem.Controllers;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
    public class EventFeedbackControllerTests
    {
        private readonly Mock<IFeedbackService> _mockFeedbackService;
        private readonly Mock<IEventService> _mockEventService;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

        public EventFeedbackControllerTests()
        {
            _mockFeedbackService = new Mock<IFeedbackService>();
            _mockEventService = new Mock<IEventService>();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
        }

        private ClaimsPrincipal CreateUserPrincipal(string userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuthentication"));
        }

        [Fact]
        public async Task Index_GET_ReturnsNotFound_WhenEventDoesNotExist()
        {
            
            _mockEventService.Setup(s => s.GetEventDetailsAsync(1)).ReturnsAsync((Event)null);

            var controller = new EventFeedbackController(
                _mockFeedbackService.Object,
                _mockEventService.Object,
                _mockUserManager.Object);

            
            var result = await controller.Index(1);

            
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Event not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task Index_GET_ReturnsViewResult_WithFeedbackViewModel()
        {
            
            var eventDetails = new Event { Id = 1, Name = "Event 1" };
            var feedbacks = new List<Feedback>
        {
            new Feedback { Id = 1, FeedbackContent = "Great event!", UserId = "user1" }
        };

            _mockEventService.Setup(s => s.GetEventDetailsAsync(1)).ReturnsAsync(eventDetails);
            _mockFeedbackService.Setup(s => s.GetFeedbacksByEventIdAsync(1)).ReturnsAsync(feedbacks);

            var controller = new EventFeedbackController(
                _mockFeedbackService.Object,
                _mockEventService.Object,
                _mockUserManager.Object);

            
            var result = await controller.Index(1);

            
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<FeedbackViewModel>(viewResult.Model);

            Assert.Equal(1, model.EventId);
            Assert.Equal("Event 1", model.EventName);
            Assert.Single(model.Feedbacks);
            Assert.Equal("Great event!", model.Feedbacks.First().FeedbackContent);
        }
        [Fact]
        public async Task Index_POST_RedirectsToLogin_WhenUserIsNotAuthenticated()
        {
            
            var model = new FeedbackViewModel { EventId = 1 };

            var controller = new EventFeedbackController(
                _mockFeedbackService.Object,
                _mockEventService.Object,
                _mockUserManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = null 
                    }
                }
            };

            
            var result = await controller.Index(model);

            
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal("/Identity/Account/Login", redirectResult.Url);
        }


        [Fact]
        public async Task Index_POST_ReturnsUnauthorized_WhenUserNotFound()
        {
            
            var model = new FeedbackViewModel { EventId = 1 };

            _mockUserManager.Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser)null);

            var controller = new EventFeedbackController(
                _mockFeedbackService.Object,
                _mockEventService.Object,
                _mockUserManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateUserPrincipal("user1")
                    }
                }
            };

            
            var result = await controller.Index(model);

           
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Index_POST_RedirectsToIndex_WhenModelStateIsValid()
        {
            
            var model = new FeedbackViewModel
            {
                EventId = 1,
                NewFeedback = new Feedback { FeedbackContent = "Great event!" }
            };

            var user = new ApplicationUser { Id = "user1" };

            _mockUserManager.Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockFeedbackService.Setup(s => s.AddFeedbackAsync(It.IsAny<Feedback>())).Returns(Task.CompletedTask);

            var controller = new EventFeedbackController(
                _mockFeedbackService.Object,
                _mockEventService.Object,
                _mockUserManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateUserPrincipal("user1")
                    }
                }
            };

            
            var result = await controller.Index(model);

            
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(controller.Index), redirectResult.ActionName);
            Assert.Equal(1, redirectResult.RouteValues["eventId"]);

            _mockFeedbackService.Verify(s => s.AddFeedbackAsync(It.IsAny<Feedback>()), Times.Once);
        }

        [Fact]
        public async Task Index_POST_ReturnsView_WithValidationErrors_WhenModelStateIsInvalid()
        {
            
            var model = new FeedbackViewModel { EventId = 1 };

            var eventDetails = new Event { Id = 1, Name = "Event 1" };
            var feedbacks = new List<Feedback>();

            _mockEventService.Setup(s => s.GetEventDetailsAsync(1)).ReturnsAsync(eventDetails);
            _mockFeedbackService.Setup(s => s.GetFeedbacksByEventIdAsync(1)).ReturnsAsync(feedbacks);

            var controller = new EventFeedbackController(
                _mockFeedbackService.Object,
                _mockEventService.Object,
                _mockUserManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateUserPrincipal("user1")
                    }
                }
            };

            controller.ModelState.AddModelError("Error", "Invalid data");

            
            var result = await controller.Index(model);

            
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsAssignableFrom<FeedbackViewModel>(viewResult.Model);

            Assert.Equal("Event 1", returnedModel.EventName);
            Assert.Empty(returnedModel.Feedbacks);
        }
    }
}
