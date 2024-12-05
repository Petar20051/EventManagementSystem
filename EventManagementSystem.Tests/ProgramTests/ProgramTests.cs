using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ProgramTests
{
    public class ProgramTests
    {
        [Fact]
        public void Program_Main_SetsUpBuilderCorrectly()
        {
            // Arrange
            var builder = WebApplication.CreateBuilder();

            // Add in-memory database for testing
            builder.Services.AddDbContext<EventDbContext>(options =>
                options.UseInMemoryDatabase("TestDatabase"));

            // Mock UserManager<ApplicationUser>
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object,
                null, null, null, null, null, null, null, null);
            builder.Services.AddSingleton(userManagerMock.Object);

            // Mock RoleManager<IdentityRole>
            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            var roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                roleStoreMock.Object,
                null, null, null, null);
            builder.Services.AddSingleton(roleManagerMock.Object);

            // Add other services required by Program.RegisterCustomServices
            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication("TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
            builder.Services.AddSignalR(); // Add SignalR
            builder.Services.AddLogging(); // Add Logging

            // Act
            Program.RegisterCustomServices(builder.Services);

            // Assert
            var serviceProvider = builder.Services.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<IEventService>()); // Check EventService registration
            Assert.NotNull(serviceProvider.GetService<UserManager<ApplicationUser>>()); // Check UserManager
        }
    }

  
   

}
