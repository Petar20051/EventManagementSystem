using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Extensions;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ProgramTests
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void Services_AreRegisteredCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();

            // Mock IConfiguration
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                { "Stripe:ApiKey", "test_key" },
                { "Stripe:WebhookSecret", "test_secret" }
                })
                .Build();
            services.AddSingleton<IConfiguration>(configuration);

            // Mock UserManager and RoleManager
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object,
                null, null, null, null, null, null, null, null);
            services.AddSingleton(userManagerMock.Object);

            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            var roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                roleStoreMock.Object,
                null, null, null, null);
            services.AddSingleton(roleManagerMock.Object);

            // Add an in-memory database for EventDbContext
            services.AddDbContext<EventDbContext>(options =>
                options.UseInMemoryDatabase("TestDatabase"));

            // Register a real LoggerFactory
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

            // Add SignalR (this will internally register DefaultHubLifetimeManager)
            services.AddSignalR();

            // Act
            Program.RegisterCustomServices(services);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(serviceProvider.GetService<IProfileService>());
            Assert.NotNull(serviceProvider.GetService<IEventService>());
            Assert.NotNull(serviceProvider.GetService<IVenueService>());
            Assert.NotNull(serviceProvider.GetService<IStripePaymentService>());
            Assert.NotNull(serviceProvider.GetService<IPaymentService>());
            Assert.NotNull(serviceProvider.GetService<ISponsorshipService>());
            Assert.NotNull(serviceProvider.GetService<INotificationService>());
            Assert.NotNull(serviceProvider.GetService<IDiscountService>());
            Assert.NotNull(serviceProvider.GetService<IPaymentMethodServiceWrapper>());
            Assert.NotNull(serviceProvider.GetService<IPaymentMethodService>());
        }
    }

}
