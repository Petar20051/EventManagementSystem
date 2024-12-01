using EventMaganementSystem.Controllers;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Infrastructure.Entities;
using EventManagementSystem.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ControllersTests
{
    public class AdminControllerTests
    {
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

        public AdminControllerTests()
        {
            _mockNotificationService = new Mock<INotificationService>();

            var mockUserStore = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                mockUserStore.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task SendGeneralNotification_ReturnsIndexView_WhenMessageIsNullOrEmpty()
        {
            // Arrange
            var controller = new AdminController(_mockNotificationService.Object, _mockUserManager.Object);
            string message = null;

            // Act
            var result = await controller.SendGeneralNotification(message);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", viewResult.ViewName);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains(controller.ModelState, kvp => kvp.Key == "" && kvp.Value.Errors.Any());
        }

        

        [Fact]
        public async Task SendSystemAlert_ReturnsIndexView_WhenAlertMessageIsNullOrEmpty()
        {
            // Arrange
            var controller = new AdminController(_mockNotificationService.Object, _mockUserManager.Object);
            string alertMessage = null;

            // Act
            var result = await controller.SendSystemAlert(alertMessage);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", viewResult.ViewName);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains(controller.ModelState, kvp => kvp.Key == "" && kvp.Value.Errors.Any());
        }

    }
}
