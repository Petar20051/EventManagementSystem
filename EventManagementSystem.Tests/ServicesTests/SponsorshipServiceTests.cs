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
        private readonly Mock<IStripePaymentService> _mockStripePaymentService;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly SponsorshipService _sponsorshipService;

        public SponsorshipServiceTests()
        {
            _mockStripePaymentService = new Mock<IStripePaymentService>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _sponsorshipService = new SponsorshipService(null, _mockStripePaymentService.Object);
        }

        // Test: Payment process succeeds, and the sponsored amount is updated
        [Fact]
        public async Task ProcessSponsorshipAsync_ShouldUpdateSponsoredAmount_WhenPaymentSucceeds()
        {
            var user = new ApplicationUser { Id = "test-user-id", SponsoredAmount = 0m };
            var amount = 50m;
            var selectedCardId = "card_12345";

            // Simulate a successful payment
            _mockStripePaymentService.Setup(s => s.ProcessPaymentAsync(amount, selectedCardId, user.Id))
                                      .ReturnsAsync("succeeded");

            await _sponsorshipService.ProcessSponsorshipAsync(user, amount, selectedCardId);

            Assert.Equal(amount, user.SponsoredAmount); // Amount should be updated
        }

        // Test: Payment fails and throws an exception
        [Fact]
        public async Task ProcessSponsorshipAsync_ShouldThrowException_WhenPaymentFails()
        {
            var user = new ApplicationUser { Id = "test-user-id", SponsoredAmount = 0m };
            var amount = 50m;
            var selectedCardId = "card_12345";

            _mockStripePaymentService.Setup(s => s.ProcessPaymentAsync(amount, selectedCardId, user.Id))
                                      .ReturnsAsync("failed");

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _sponsorshipService.ProcessSponsorshipAsync(user, amount, selectedCardId));

            Assert.Equal("Payment failed. Please try again.", exception.Message);
        }

        // Test: Unexpected payment status should throw an exception
        
    }
}
