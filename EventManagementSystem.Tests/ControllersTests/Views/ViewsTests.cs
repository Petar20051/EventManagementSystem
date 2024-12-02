using EventMaganementSystem.Controllers;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models;
using EventManagementSystem.Core.Models.Account;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ControllersTests.Views
{
    public class ViewsTests
    {



        [Fact]
        public async Task CalendarPage_ShouldRenderCorrectly()
        {
            // Arrange
            var factory = new WebApplicationFactory<Program>(); // Replace `Program` with your actual Startup class
            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/Calendar");
            var htmlContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode(); // Status code 200-299
            Assert.Contains("<h2 class=\"text-center mb-4\" style=\"color: #004080;\">Event Calendar</h2>", htmlContent);
            Assert.Contains("<div id=\"calendar\"></div>", htmlContent);
            Assert.Contains("https://cdn.jsdelivr.net/npm/fullcalendar@5.1.0/main.min.css", htmlContent); // Validate dependency inclusion
            Assert.Contains("loadData()", htmlContent); // Validate JavaScript initialization
        }

        [Fact]
        public async Task CalendarPage_RendersCorrectly_WithEvents()
        {
            // Arrange
            var factory = new WebApplicationFactory<Program>(); // Replace Program with your actual entry class (Program.cs or Startup.cs)
            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/Calendar"); // Corrected to match the controller's route
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Ensure 200 OK
            Assert.Contains("<h2 class=\"text-center mb-4\" style=\"color: #004080;\">Event Calendar</h2>", responseContent);
            Assert.Contains("<div id=\"calendar\"></div>", responseContent);
        }

        [Fact]
        public async Task IndexAsync_ReturnsViewWithModel_WhenUserExists()
        {
            // Arrange
            var userId = "test_user";
            var mockProfileService = new Mock<IProfileService>();
            mockProfileService.Setup(service => service.GetUserAsync(userId))
                .ReturnsAsync(new ApplicationUser { Id = userId });

            var controller = new ChangePasswordController(mockProfileService.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }))
                }
            };

            // Act
            var result = await controller.IndexAsync();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ChangePasswordViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task IndexAsync_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = "test_user";
            var mockProfileService = new Mock<IProfileService>();
            mockProfileService.Setup(service => service.GetUserAsync(userId))
                .ReturnsAsync((ApplicationUser)null);

            var controller = new ChangePasswordController(mockProfileService.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }))
                }
            };

            // Act
            var result = await controller.IndexAsync();

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

    }
}
