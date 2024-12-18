﻿using EventManagementSystem.Core.Services;
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
    public class UserServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _userService = new UserService(_mockUserManager.Object);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            
            var userId = "test-user-id";
            var expectedUser = new ApplicationUser { Id = userId, UserName = "TestUser" };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                            .ReturnsAsync(expectedUser);

            
            var actualUser = await _userService.GetUserByIdAsync(userId);

            
            Assert.NotNull(actualUser);
            Assert.Equal(expectedUser, actualUser);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            
            var userId = "non-existent-user";

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                            .ReturnsAsync((ApplicationUser)null);

            
            var actualUser = await _userService.GetUserByIdAsync(userId);

            
            Assert.Null(actualUser);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetUserByIdAsync_ShouldThrowException_WhenUserIdIsNullOrEmpty(string userId)
        {
            
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.GetUserByIdAsync(userId));

            Assert.Equal("User ID cannot be null or empty. (Parameter 'userId')", exception.Message);
        }

        [Fact]
        public async Task GetStripeCustomerIdAsync_ShouldReturnStripeCustomerId_WhenUserExists()
        {
            
            var userId = "test-user-id";
            var expectedCustomerId = "cus_12345";
            var user = new ApplicationUser { Id = userId, StripeCustomerId = expectedCustomerId };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                            .ReturnsAsync(user);

            
            var actualCustomerId = await _userService.GetStripeCustomerIdAsync(userId);

            
            Assert.Equal(expectedCustomerId, actualCustomerId);
        }

        [Fact]
        public async Task GetStripeCustomerIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            
            var userId = "non-existent-user";

            _mockUserManager.Setup(um => um.FindByIdAsync(It.Is<string>(id => id == userId)))
                            .ReturnsAsync((ApplicationUser)null);

            
            var actualCustomerId = await _userService.GetStripeCustomerIdAsync(userId);

            
            Assert.Null(actualCustomerId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetStripeCustomerIdAsync_ShouldThrowException_WhenUserIdIsNullOrEmpty(string userId)
        {
            
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.GetStripeCustomerIdAsync(userId));

            Assert.Equal("User ID cannot be null or empty. (Parameter 'userId')", exception.Message);
        }

        [Fact]
        public async Task SaveStripeCustomerIdAsync_ShouldUpdateStripeCustomerId_WhenUserExists()
        {
            
            var userId = "test-user-id";
            var stripeCustomerId = "cus_12345";
            var user = new ApplicationUser { Id = userId };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                            .ReturnsAsync(user);

            _mockUserManager.Setup(um => um.UpdateAsync(user))
                            .ReturnsAsync(IdentityResult.Success);

            
            await _userService.SaveStripeCustomerIdAsync(userId, stripeCustomerId);

            
            Assert.Equal(stripeCustomerId, user.StripeCustomerId);
            _mockUserManager.Verify(um => um.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task SaveStripeCustomerIdAsync_ShouldNotCallUpdate_WhenUserDoesNotExist()
        {
            
            var userId = "non-existent-user";
            var stripeCustomerId = "cus_12345";

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                            .ReturnsAsync((ApplicationUser)null); 

            
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userService.SaveStripeCustomerIdAsync(userId, stripeCustomerId));

            Assert.Equal("User not found.", exception.Message);
            _mockUserManager.Verify(um => um.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never); 
        }

        [Fact]
        public async Task SaveStripeCustomerIdAsync_ShouldThrowException_WhenUpdateFails()
        {
            
            var userId = "test-user-id";
            var stripeCustomerId = "cus_12345";
            var user = new ApplicationUser { Id = userId };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                            .ReturnsAsync(user);

            _mockUserManager.Setup(um => um.UpdateAsync(user))
                            .ReturnsAsync(IdentityResult.Failed(new IdentityError
                            {
                                Code = "UpdateFailed",
                                Description = "Failed to update the user."
                            }));

            
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userService.SaveStripeCustomerIdAsync(userId, stripeCustomerId));

            Assert.Equal("Failed to update the user's Stripe customer ID.", exception.Message);
        }

        [Theory]
        [InlineData(null, "cus_12345")]
        [InlineData("test-user-id", null)]
        [InlineData("", "cus_12345")]
        [InlineData("test-user-id", "")]
        public async Task SaveStripeCustomerIdAsync_ShouldThrowException_WhenArgumentsAreInvalid(string userId, string stripeCustomerId)
        {
            
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.SaveStripeCustomerIdAsync(userId, stripeCustomerId));

            Assert.NotNull(exception.Message);
        }
    }
}
