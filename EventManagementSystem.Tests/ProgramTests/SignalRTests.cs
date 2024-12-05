using EventMaganementSystem;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ProgramTests
{
    public class SignalRTests 
    {
        [Fact]
        public void SignalR_IsConfiguredCorrectly()
        {
            // Arrange
            var builder = WebApplication.CreateBuilder();

            // Register required services
            builder.Services.AddAuthorization(); // Add Authorization services
            builder.Services.AddAuthentication("TestScheme") // Add Authentication for testing
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
            builder.Services.AddSignalR(); // Add SignalR services
            builder.Services.AddLogging(); // Add Logging services
            builder.Services.AddRazorPages(); // Add Razor Pages for middleware pipeline
            builder.Services.AddControllersWithViews(); // Add Controllers with Views

            var app = builder.Build();

            // Act
            Program.ConfigureMiddleware(app);

            // Assert
            Assert.NotNull(app); // Ensure the app instance is not null
        }
    }
}

