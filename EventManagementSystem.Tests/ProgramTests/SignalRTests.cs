using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ProgramTests
{
    public class SignalRTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public SignalRTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

       

        [Fact]
        public async Task DefaultRoute_RendersHomePage()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/");

            // Assert
            response.EnsureSuccessStatusCode(); // Ensure the page loaded successfully
            var pageContent = await response.Content.ReadAsStringAsync();

            // Check if "Home" is somewhere in the HTML content
            Assert.Contains("Home", pageContent, StringComparison.OrdinalIgnoreCase);
        }
    }
}

