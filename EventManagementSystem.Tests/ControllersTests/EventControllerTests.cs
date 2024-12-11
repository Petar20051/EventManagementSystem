using EventMaganementSystem.Controllers;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Events;
using EventManagementSystem.Infrastructure.Data.Enums;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ControllersTests
{
    public class EventControllerTests
    {
        private readonly Mock<IEventService> _mockEventService;
        private readonly Mock<IVenueService> _mockVenueService;
        private readonly Mock<IUserEventService> _mockUserEventService;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

        public EventControllerTests()
        {
            _mockEventService = new Mock<IEventService>();
            _mockVenueService = new Mock<IVenueService>();
            _mockUserEventService = new Mock<IUserEventService>();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
        }

        private ClaimsPrincipal CreateUserPrincipal(string userId)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userId)
    };

            
            var identity = new ClaimsIdentity(claims, "TestAuthentication");

            return new ClaimsPrincipal(identity);
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithEventViewModels()
        {
            
            var events = new List<Event>
    {
        new Event { Id = 1, Name = "Event 1", Venue = new Venue { Address = "Location 1" } },
        new Event { Id = 2, Name = "Event 2", Venue = null }
    };
            _mockEventService.Setup(s => s.GetAllEventsAsync()).ReturnsAsync(events);

            var controller = new EventsController(
                _mockEventService.Object,
                _mockVenueService.Object,
                _mockUserEventService.Object,
                _mockUserManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                    new Claim(ClaimTypes.NameIdentifier, "user1")
                }))
                    }
                }
            };

            
            var result = await controller.Index();

            
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<EventViewModel>>(viewResult.Model);

            Assert.Equal(2, model.Count);
            Assert.Equal("Event 1", model[0].Name);
            Assert.Equal("Location 1", model[0].Location);
            Assert.Equal("Unknown Location", model[1].Location);

            _mockEventService.Verify(s => s.GetAllEventsAsync(), Times.Once);
        }


        [Fact]
        public async Task Create_GET_ReturnsViewResult_WithVenues()
        {
            var venues = new List<Venue>
        {
            new Venue { Id = 1, Name = "Venue 1" },
            new Venue { Id = 2, Name = "Venue 2" }
        };
            _mockVenueService.Setup(s => s.GetAllVenuesAsync()).ReturnsAsync(venues);

            var controller = new EventsController(_mockEventService.Object, _mockVenueService.Object, _mockUserEventService.Object, _mockUserManager.Object);

            var result = await controller.Create();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<CreateEventViewModel>(viewResult.Model);

            Assert.Equal(2, model.Venues.Count);
            Assert.Equal("Venue 1", model.Venues[0].Name);
        }

        

        [Fact]
        public async Task Create_POST_ReturnsView_WhenModelStateIsInvalid()
        {
            
            var model = new CreateEventViewModel();

            var venues = new List<Venue>
    {
        new Venue { Id = 1, Name = "Venue 1" },
        new Venue { Id = 2, Name = "Venue 2" }
    };

            _mockVenueService.Setup(s => s.GetAllVenuesAsync()).ReturnsAsync(venues);

            var controller = new EventsController(
                _mockEventService.Object,
                _mockVenueService.Object,
                _mockUserEventService.Object,
                _mockUserManager.Object);

            controller.ModelState.AddModelError("Error", "Invalid data");

            
            var result = await controller.Create(model);

            
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsAssignableFrom<CreateEventViewModel>(viewResult.Model);

            Assert.Equal(model.Name, returnedModel.Name); 
            Assert.Equal(2, returnedModel.Venues.Count); 
            Assert.Equal("Venue 1", returnedModel.Venues[0].Name); 
        }


        [Fact]
        public async Task Edit_GET_ReturnsNotFound_WhenEventDoesNotExist()
        {
            _mockEventService.Setup(s => s.GetEventByIdAsync(1)).ReturnsAsync((Event)null);

            var controller = new EventsController(_mockEventService.Object, _mockVenueService.Object, _mockUserEventService.Object, _mockUserManager.Object);

            var result = await controller.Edit(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_GET_ReturnsViewResult_WithEventViewModel()
        {
            var eventItem = new Event
            {
                Id = 1,
                Name = "Event 1",
                VenueId = 1,
                OrganizerId = "user1"
            };
            var venues = new List<Venue>
        {
            new Venue { Id = 1, Name = "Venue 1" }
        };

            _mockEventService.Setup(s => s.GetEventByIdAsync(1)).ReturnsAsync(eventItem);
            _mockVenueService.Setup(s => s.GetAllVenuesAsync()).ReturnsAsync(venues);

            var controller = new EventsController(_mockEventService.Object, _mockVenueService.Object, _mockUserEventService.Object, _mockUserManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateUserPrincipal("user1")
                    }
                }
            };

            var result = await controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<EventViewModel>(viewResult.Model);

            Assert.Equal("Event 1", model.Name);
            Assert.Single(model.Venues);
            Assert.Equal("Venue 1", model.Venues[0].Name);
        }

        


        [Fact]
        public async Task Details_ReturnsNotFound_WhenEventDoesNotExist()
        {
            
            _mockEventService.Setup(s => s.GetEventByIdAsync(1)).ReturnsAsync((Event)null);

            var controller = new EventsController(
                _mockEventService.Object,
                _mockVenueService.Object,
                _mockUserEventService.Object,
                _mockUserManager.Object);

            
            var result = await controller.Details(1);

            
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Event not found", notFoundResult.Value);

            _mockEventService.Verify(s => s.GetEventByIdAsync(1), Times.Once);
        }


        [Fact]
        public async Task Details_ReturnsViewResult_WithEventDetails()
        {
            var eventItem = new Event
            {
                Id = 1,
                Name = "Event 1",
                Date = DateTime.Now,
                VenueId = 1,
                OrganizerId = "user1"
            };

            var venue = new Venue
            {
                Id = 1,
                Name = "Venue 1",
                Address = "123 Main St"
            };

            _mockEventService.Setup(s => s.GetEventByIdAsync(1)).ReturnsAsync(eventItem);
            _mockVenueService.Setup(s => s.GetVenueByIdAsync(1)).ReturnsAsync(venue);

            var controller = new EventsController(_mockEventService.Object, _mockVenueService.Object, _mockUserEventService.Object, _mockUserManager.Object);

            var result = await controller.Details(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<EventDetailsViewModel>(viewResult.Model);

            Assert.Equal("Event 1", model.Name);
            Assert.Equal("123 Main St", model.Address);
        }
    }
}
