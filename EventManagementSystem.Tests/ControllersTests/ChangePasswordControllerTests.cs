using EventMaganementSystem.Controllers;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Account;
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
    public class ChangePasswordControllerTests
    {
        private readonly Mock<IProfileService> _mockProfileService;

        public ChangePasswordControllerTests()
        {
            _mockProfileService = new Mock<IProfileService>();
        }

        private ClaimsPrincipal CreateUserPrincipal(string userId)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
            return new ClaimsPrincipal(new ClaimsIdentity(claims));
        }

        [Fact]
        public async Task IndexAsync_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = "user1";
            _mockProfileService.Setup(s => s.GetUserAsync(userId)).ReturnsAsync((ApplicationUser)null);

            var controller = new ChangePasswordController(_mockProfileService.Object)
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
            var result = await controller.IndexAsync();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User not found", notFoundResult.Value);
        }

        [Fact]
        public async Task IndexAsync_ReturnsViewResult_WhenUserExists()
        {
            // Arrange
            var userId = "user1";
            var user = new ApplicationUser { Id = userId };
            _mockProfileService.Setup(s => s.GetUserAsync(userId)).ReturnsAsync(user);

            var controller = new ChangePasswordController(_mockProfileService.Object)
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
            var result = await controller.IndexAsync();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ChangePasswordViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Index_ReturnsView_WhenModelStateIsInvalid()
        {
            // Arrange
            var model = new ChangePasswordViewModel();
            var controller = new ChangePasswordController(_mockProfileService.Object);
            controller.ModelState.AddModelError("Error", "Invalid data");

            // Act
            var result = await controller.Index(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task Index_RedirectsToHome_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = "user1";
            var model = new ChangePasswordViewModel
            {
                CurrentPassword = "oldPass",
                NewPassword = "newPass"
            };
            _mockProfileService.Setup(s => s.GetUserAsync(userId)).ReturnsAsync((ApplicationUser)null);

            var controller = new ChangePasswordController(_mockProfileService.Object)
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
            var result = await controller.Index(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Index_RedirectsToManageProfile_WhenPasswordChangeSucceeds()
        {
            // Arrange
            var userId = "user1";
            var user = new ApplicationUser { Id = userId };
            var model = new ChangePasswordViewModel
            {
                CurrentPassword = "oldPass",
                NewPassword = "newPass"
            };

            _mockProfileService.Setup(s => s.GetUserAsync(userId)).ReturnsAsync(user);
            _mockProfileService.Setup(s => s.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword))
                               .ReturnsAsync(IdentityResult.Success);

            var controller = new ChangePasswordController(_mockProfileService.Object)
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
            var result = await controller.Index(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("ManageProfile", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Index_AddsErrorsToModelState_WhenPasswordChangeFails()
        {
            // Arrange
            var userId = "user1";
            var user = new ApplicationUser { Id = userId };
            var model = new ChangePasswordViewModel
            {
                CurrentPassword = "oldPass",
                NewPassword = "newPass"
            };

            var identityErrors = new[] { new IdentityError { Description = "Error 1" }, new IdentityError { Description = "Error 2" } };
            _mockProfileService.Setup(s => s.GetUserAsync(userId)).ReturnsAsync(user);
            _mockProfileService.Setup(s => s.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword))
                               .ReturnsAsync(IdentityResult.Failed(identityErrors));

            var controller = new ChangePasswordController(_mockProfileService.Object)
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
            var result = await controller.Index(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.True(controller.ModelState.ContainsKey(string.Empty));
            Assert.Equal(2, controller.ModelState[string.Empty].Errors.Count);
            Assert.Equal("Error 1", controller.ModelState[string.Empty].Errors[0].ErrorMessage);
            Assert.Equal("Error 2", controller.ModelState[string.Empty].Errors[1].ErrorMessage);
        }
    }
}
