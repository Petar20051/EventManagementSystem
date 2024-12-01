using EventManagementSystem.Core.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ProgramTests
{
    public class DependencyInjectionTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public DependencyInjectionTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public void AllServices_AreRegisteredCorrectly()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            // Act & Assert
            Assert.NotNull(serviceProvider.GetRequiredService<IProfileService>());
            Assert.NotNull(serviceProvider.GetRequiredService<IEventService>());
            Assert.NotNull(serviceProvider.GetRequiredService<IVenueService>());
            Assert.NotNull(serviceProvider.GetRequiredService<IUserEventService>());
            Assert.NotNull(serviceProvider.GetRequiredService<ISponsorshipService>());
            Assert.NotNull(serviceProvider.GetRequiredService<INotificationService>());
            Assert.NotNull(serviceProvider.GetRequiredService<IStripePaymentService>());
        }
    }
}
