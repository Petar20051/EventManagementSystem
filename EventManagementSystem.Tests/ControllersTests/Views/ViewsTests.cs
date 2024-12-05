using EventMaganementSystem.Controllers;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models;
using EventManagementSystem.Core.Models.Account;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ControllersTests.Views
{
    public class ViewsTests
    {

        [Fact]
        public async Task IndexAsync_ReturnsViewWithModel_WhenUserExists()
        {
            // Arrange
            var userId = "test_user";
            var mockProfileService = new Mock<IProfileService>();
            mockProfileService.Setup(service => service.GetUserAsync(userId))
                .ReturnsAsync(new ApplicationUser { Id = userId });

            var controller = new ChangePasswordController(mockProfileService.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }))
                }
            };

            // Act
            var result = await controller.IndexAsync();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ChangePasswordViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task IndexAsync_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = "test_user";
            var mockProfileService = new Mock<IProfileService>();
            mockProfileService.Setup(service => service.GetUserAsync(userId))
                .ReturnsAsync((ApplicationUser)null);

            var controller = new ChangePasswordController(mockProfileService.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }))
                }
            };

            // Act
            var result = await controller.IndexAsync();

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

    }
}
