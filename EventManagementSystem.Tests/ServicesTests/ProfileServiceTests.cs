using EventManagementSystem.Core.Services;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ServicesTests
{
    public class ProfileServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly ProfileService _profileService;

        public ProfileServiceTests()
        {
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null
            );
            _profileService = new ProfileService(_mockUserManager.Object);
        }

        [Fact]
        public async Task GetUserAsync_ShouldReturnUser_WhenUserExists()
        {
            
            var userId = "user1";
            var user = new ApplicationUser { Id = userId, UserName = "testuser" };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                            .ReturnsAsync(user);

            
            var result = await _profileService.GetUserAsync(userId);

            
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal("testuser", result.UserName);
        }

        [Fact]
        public async Task GetUserAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            
            var userId = "nonexistent";

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                            .ReturnsAsync((ApplicationUser)null);

            
            var result = await _profileService.GetUserAsync(userId);

            
            Assert.Null(result);
        }

        [Fact]
        public async Task ChangePasswordAsync_ShouldReturnSuccessResult_WhenPasswordIsChanged()
        {
            
            var user = new ApplicationUser { Id = "user1", UserName = "testuser" };
            var currentPassword = "OldPassword123!";
            var newPassword = "NewPassword123!";

            _mockUserManager.Setup(um => um.ChangePasswordAsync(user, currentPassword, newPassword))
                            .ReturnsAsync(IdentityResult.Success);

            
            var result = await _profileService.ChangePasswordAsync(user, currentPassword, newPassword);

            
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task ChangePasswordAsync_ShouldReturnFailureResult_WhenPasswordChangeFails()
        {
            
            var user = new ApplicationUser { Id = "user1", UserName = "testuser" };
            var currentPassword = "WrongPassword123!";
            var newPassword = "NewPassword123!";
            var identityError = new IdentityError { Code = "PasswordMismatch", Description = "Current password is incorrect." };

            _mockUserManager.Setup(um => um.ChangePasswordAsync(user, currentPassword, newPassword))
                            .ReturnsAsync(IdentityResult.Failed(identityError));

            
            var result = await _profileService.ChangePasswordAsync(user, currentPassword, newPassword);

            
            Assert.False(result.Succeeded);
            Assert.Single(result.Errors);
            Assert.Equal("PasswordMismatch", result.Errors.First().Code);
        }
    }
}
