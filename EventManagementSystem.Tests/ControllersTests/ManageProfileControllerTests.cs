using EventMaganementSystem.Controllers;
using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Account;
using EventManagementSystem.Infrastructure.Data.Enums;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ControllersTests
{
    public class ManageProfileControllerTests
    {
        private readonly Mock<IProfileService> _mockProfileService;
        private readonly EventDbContext _dbContext;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

        public ManageProfileControllerTests()
        {
            _mockProfileService = new Mock<IProfileService>();

            var options = new DbContextOptionsBuilder<EventDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new EventDbContext(options);

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            // Seed database
            _dbContext.Tickets.Add(new Ticket { Id = 1, HolderId = "user1", EventId = 1 });
            _dbContext.SaveChanges();
        }

        private ClaimsPrincipal CreateUserPrincipal(string userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuthentication"));
        }

        [Fact]
        public async Task Index_RedirectsToLogin_WhenUserNotFound()
        {
            // Arrange
            _mockProfileService.Setup(s => s.GetUserAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);

            var controller = new ManageProfileController(_mockProfileService.Object, _dbContext, _mockUserManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateUserPrincipal("user1")
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
        public async Task Index_ReturnsViewResult_WithManageProfileViewModel()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "TestUser",
                Email = "testuser@example.com",
                SponsoredAmount = 100,
                SponsorshipTier = SponsorshipTier.Silver
            };

            _mockProfileService.Setup(s => s.GetUserAsync("user1")).ReturnsAsync(user);

            var controller = new ManageProfileController(_mockProfileService.Object, _dbContext, _mockUserManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateUserPrincipal("user1")
                    }
                }
            };

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ManageProfileViewModel>(viewResult.Model);

            Assert.Equal("TestUser", model.UserName);
            Assert.Equal("testuser@example.com", model.Email);
            Assert.Equal(100, model.SponsoredAmount);
            Assert.Equal("Silver", model.SponsorshipTier);
            Assert.Single(model.Tickets);
        }

        [Fact]
        public async Task BecomeOrganizer_AddsUserToRole_WhenNotAlreadyOrganizer()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user1", UserName = "TestUser" };

            _mockUserManager.Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockUserManager.Setup(s => s.IsInRoleAsync(user, "Organizer")).ReturnsAsync(false);
            _mockUserManager.Setup(s => s.AddToRoleAsync(user, "Organizer")).ReturnsAsync(IdentityResult.Success);

            var controller = new ManageProfileController(_mockProfileService.Object, _dbContext, _mockUserManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateUserPrincipal("user1")
                    }
                },
                TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>()) // Initialize TempData
            };

            // Act
            var result = await controller.BecomeOrganizer();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("You are now an Organizer!", controller.TempData["Message"]);

            _mockUserManager.Verify(s => s.AddToRoleAsync(user, "Organizer"), Times.Once);
        }


        [Fact]
        public async Task BecomeOrganizer_DoesNotAddUserToRole_WhenAlreadyOrganizer()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user1", UserName = "TestUser" };

            _mockUserManager.Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockUserManager.Setup(s => s.IsInRoleAsync(user, "Organizer")).ReturnsAsync(true);

            var controller = new ManageProfileController(_mockProfileService.Object, _dbContext, _mockUserManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateUserPrincipal("user1")
                    }
                },
                TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>()) // Initialize TempData
            };

            // Act
            var result = await controller.BecomeOrganizer();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("You are already an Organizer.", controller.TempData["Error"]);

            _mockUserManager.Verify(s => s.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }
        [Fact]
        public async Task BecomeOrganizer_ReturnsError_WhenAddingToRoleFails()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user1", UserName = "TestUser" };

            _mockUserManager.Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockUserManager.Setup(s => s.IsInRoleAsync(user, "Organizer")).ReturnsAsync(false);
            _mockUserManager.Setup(s => s.AddToRoleAsync(user, "Organizer")).ReturnsAsync(IdentityResult.Failed());

            var controller = new ManageProfileController(_mockProfileService.Object, _dbContext, _mockUserManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateUserPrincipal("user1")
                    }
                },
                TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>()) // Initialize TempData
            };

            // Act
            var result = await controller.BecomeOrganizer();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Failed to assign Organizer role.", controller.TempData["Error"]);

            _mockUserManager.Verify(s => s.AddToRoleAsync(user, "Organizer"), Times.Once);
        }

    }
}
