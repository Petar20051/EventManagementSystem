using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Services;
using EventManagementSystem.Infrastructure.Data.Enums;
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
    public class SponsorshipServiceTests
    {
        private readonly Mock<IPaymentService> _mockPaymentService;
        private readonly Mock<IStripePaymentService> _mockStripePaymentService;
        private readonly SponsorshipService _sponsorshipService;

        public SponsorshipServiceTests()
        {
            _mockPaymentService = new Mock<IPaymentService>();
            _mockStripePaymentService = new Mock<IStripePaymentService>();

            _sponsorshipService = new SponsorshipService(_mockPaymentService.Object, _mockStripePaymentService.Object);
        }

        [Fact]
        public async Task ProcessSponsorshipAsync_SuccessfullyProcessesPayment()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user1", SponsoredAmount = 0 };
            var amount = 50m;
            var selectedCardId = "card1";

            _mockStripePaymentService.Setup(s => s.ProcessPaymentAsync(amount, selectedCardId, user.Id))
                .ReturnsAsync("succeeded");

            // Act
            await _sponsorshipService.ProcessSponsorshipAsync(user, amount, selectedCardId);

            // Assert
            Assert.Equal(50m, user.SponsoredAmount);
            _mockStripePaymentService.Verify(s => s.ProcessPaymentAsync(amount, selectedCardId, user.Id), Times.Once);
        }

        [Fact]
        public async Task ProcessSponsorshipAsync_ThrowsExceptionOnPaymentFailure()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user1", SponsoredAmount = 0 };
            var amount = 50m;
            var selectedCardId = "card1";

            _mockStripePaymentService.Setup(s => s.ProcessPaymentAsync(amount, selectedCardId, user.Id))
                .ReturnsAsync("failed");

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sponsorshipService.ProcessSponsorshipAsync(user, amount, selectedCardId));
        }

        [Fact]
        public async Task UpdateSponsorshipTierAsync_UpdatesUserTier()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user1", SponsoredAmount = 100, SponsorshipTier = SponsorshipTier.Silver };

            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            mockUserManager.Setup(um => um.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            await _sponsorshipService.UpdateSponsorshipTierAsync(user, mockUserManager.Object);

            // Assert
            Assert.Equal(SponsorshipTier.Gold, user.SponsorshipTier);
            mockUserManager.Verify(um => um.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdateSponsorshipTierAsync_DoesNotUpdateIfTierIsSame()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user1", SponsoredAmount = 50, SponsorshipTier = SponsorshipTier.Silver };

            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            // Act
            await _sponsorshipService.UpdateSponsorshipTierAsync(user, mockUserManager.Object);

            // Assert
            Assert.Equal(SponsorshipTier.Silver, user.SponsorshipTier);
            mockUserManager.Verify(um => um.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Theory]
        [InlineData(0, SponsorshipTier.None)]
        [InlineData(1, SponsorshipTier.Bronze)]
        [InlineData(20, SponsorshipTier.Silver)]
        [InlineData(100, SponsorshipTier.Gold)]
        public void DetermineSponsorshipTier_ReturnsCorrectTier(decimal sponsoredAmount, SponsorshipTier expectedTier)
        {
            // Act
            var result = _sponsorshipService.DetermineSponsorshipTier(sponsoredAmount);

            // Assert
            Assert.Equal(expectedTier, result);
        }
    }
}
