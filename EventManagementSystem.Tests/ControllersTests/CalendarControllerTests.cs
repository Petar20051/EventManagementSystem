using EventMaganementSystem.Controllers;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ControllersTests
{
    public class CalendarControllerTests
    {
        private readonly Mock<IEventService> _mockEventService;

        public CalendarControllerTests()
        {
            _mockEventService = new Mock<IEventService>();
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithEventsInViewData()
        {
            
            var events = new List<Event>
        {
            new Event { Id = 1, Name = "Event 1" },
            new Event { Id = 2, Name = "Event 2" }
        };

            _mockEventService.Setup(service => service.GetAllEventsAsync())
                             .ReturnsAsync(events);

            var controller = new CalendarController(_mockEventService.Object);

            
            var result = await controller.Index();

            
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(viewResult.ViewData.ContainsKey("events"));
            Assert.Equal(events, viewResult.ViewData["events"]);
            _mockEventService.Verify(service => service.GetAllEventsAsync(), Times.Once);
        }

        [Fact]
        public async Task Index_ReturnsEmptyViewData_WhenNoEventsExist()
        {
            
            var events = new List<Event>(); 

            _mockEventService.Setup(service => service.GetAllEventsAsync())
                             .ReturnsAsync(events);

            var controller = new CalendarController(_mockEventService.Object);

            
            var result = await controller.Index();

            
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(viewResult.ViewData.ContainsKey("events"));
            Assert.Empty((IEnumerable<Event>)viewResult.ViewData["events"]);
            _mockEventService.Verify(service => service.GetAllEventsAsync(), Times.Once);
        }
    }
}
