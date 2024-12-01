using EventMaganementSystem.Controllers;
using EventMaganementSystem.Models;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Home;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ControllersTests
{
    public class HomeControllerTests
    {
        private readonly Mock<ILogger<HomeController>> _mockLogger;
        private readonly Mock<IEventService> _mockEventService;

        public HomeControllerTests()
        {
            _mockLogger = new Mock<ILogger<HomeController>>();
            _mockEventService = new Mock<IEventService>();
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithUpcomingEvents()
        {
            // Arrange
            var upcomingEvents = new List<Event>
    {
        new Event { Id = 1, Name = "Event 1", Date = DateTime.UtcNow.AddDays(1) },
        new Event { Id = 2, Name = "Event 2", Date = DateTime.UtcNow.AddDays(2) }
    };

            // Explicitly pass the default value for optional parameters
            _mockEventService
                .Setup(s => s.GetUpcomingEventsAsync(5)) // Replace optional parameter with explicit value
                .ReturnsAsync(upcomingEvents);

            var controller = new HomeController(_mockLogger.Object, _mockEventService.Object);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<HomePageViewModel>(viewResult.Model);

            Assert.Equal(2, model.UpcomingEvents.Count);
            Assert.Equal("Event 1", model.UpcomingEvents[0].Name);

            _mockEventService.Verify(s => s.GetUpcomingEventsAsync(5), Times.Once); // Verify with explicit value
        }

        [Fact]
        public void Privacy_ReturnsViewResult()
        {
            // Arrange
            var controller = new HomeController(_mockLogger.Object, _mockEventService.Object);

            // Act
            var result = controller.Privacy();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Error_ReturnsViewResult_WithErrorViewModel()
        {
            // Arrange
            var controller = new HomeController(_mockLogger.Object, _mockEventService.Object);

            // Simulate a RequestId
            var requestId = "12345";
            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = requestId;

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = controller.Error();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ErrorViewModel>(viewResult.Model);

            Assert.Equal(requestId, model.RequestId);
        }
    }
}
