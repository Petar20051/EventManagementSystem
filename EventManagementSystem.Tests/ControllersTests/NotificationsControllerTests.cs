using EventMaganementSystem.Controllers;
using EventManagementSystem.Core.Contracts;
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
    public class NotificationsControllerTests
    {
        private readonly Mock<INotificationService> _mockNotificationService;

        public NotificationsControllerTests()
        {
            _mockNotificationService = new Mock<INotificationService>();
        }

        private ClaimsPrincipal CreateUserPrincipal(string userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuthentication"));
        }

        [Fact]
        public async Task Index_RedirectsToLogin_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var controller = new NotificationsController(_mockNotificationService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = null // Simulate unauthenticated user
                    }
                }
            };

            // Act
            var result = await controller.Index();

            // Assert
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal("/Identity/Account/Login", redirectResult.Url);
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithNotifications()
        {
            // Arrange
            var userId = "user1";
            var notifications = new List<Notification>
        {
            new Notification { Id = 1, Message = "Notification 1" },
            new Notification { Id = 2, Message = "Notification 2" }
        };

            _mockNotificationService.Setup(s => s.GetUserNotificationsAsync(userId)).ReturnsAsync(notifications);

            var controller = new NotificationsController(_mockNotificationService.Object)
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
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Notification>>(viewResult.Model);

            Assert.Equal(2, model.Count);
            Assert.Equal("Notification 1", model[0].Message);
            Assert.Equal("Notification 2", model[1].Message);

            _mockNotificationService.Verify(s => s.GetUserNotificationsAsync(userId), Times.Once);
        }
    }
}
