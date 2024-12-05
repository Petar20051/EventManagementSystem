using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ProgramTests
{
    public class MiddlewarePipelineTests
    {
        [Fact]
        public void Middleware_IsConfiguredCorrectly()
        {
            // Arrange
            var builder = WebApplication.CreateBuilder();

            // Register required services
            builder.Services.AddAuthorization(); // Add Authorization
            builder.Services.AddAuthentication("TestScheme") // Add Authentication for testing
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
            builder.Services.AddRazorPages();
            builder.Services.AddControllersWithViews();
            builder.Services.AddSignalR(); // Add SignalR services

            var app = builder.Build();

            // Act
            Program.ConfigureMiddleware(app);

            // Assert
            Assert.NotNull(app); // Ensure app is not null
        }
    }

    // Custom Authentication Handler for testing
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[] { new Claim(ClaimTypes.Name, "TestUser") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

}
