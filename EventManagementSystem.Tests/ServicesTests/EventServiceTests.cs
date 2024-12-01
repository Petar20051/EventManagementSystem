using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Services;
using EventManagementSystem.Infrastructure.Data.Enums;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ServicesTests
{
    public class EventServiceTests
    {

        private readonly EventService _eventService;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly EventDbContext _context;

        public EventServiceTests()
        {
            // Setup the InMemory DbContext with a unique database name for each test
            var options = new DbContextOptionsBuilder<EventDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new EventDbContext(options);

            // Mock NotificationService
            _notificationServiceMock = new Mock<INotificationService>();

            // Setup Mock UserManager<ApplicationUser>
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            // Mock Users property in UserManager (returning a collection of ApplicationUser)
            var userList = new List<ApplicationUser>
        {
            new ApplicationUser { Id = "1", UserName = "user1@example.com" },
            new ApplicationUser { Id = "2", UserName = "user2@example.com" }
        }.AsQueryable();

            _userManagerMock.Setup(um => um.Users).Returns(userList);

            // Initialize EventService with the in-memory context and mocked dependencies
            _eventService = new EventService(_context, _userManagerMock.Object, _notificationServiceMock.Object);
        }

        

        [Fact]
        public async Task GetUpcomingEventsAsync_ShouldReturnUpcomingEvents()
        {
            // Arrange
            var user = new ApplicationUser { Id = "1", UserName = "user1@example.com" };
            await _context.Users.AddAsync(user);
            var venue = new Venue { Id = 1, Name = "Venue 1", Address = "Address 1" };
            await _context.Venues.AddAsync(venue);
            await _context.SaveChangesAsync();

            var upcomingEvent = new Event
            {
                Id = 1,
                Name = "Upcoming Event",
                Date = DateTime.UtcNow.AddDays(1),
                VenueId = venue.Id,
                OrganizerId = user.Id,
                EventType = EventTypes.Conference,
                Description = "Upcoming Event Description"
            };
            var pastEvent = new Event
            {
                Id = 2,
                Name = "Past Event",
                Date = DateTime.UtcNow.AddDays(-1),
                VenueId = venue.Id,
                OrganizerId = user.Id,
                EventType = EventTypes.Conference,
                Description = "Past Event Description"
            };

            _context.Events.Add(upcomingEvent);
            _context.Events.Add(pastEvent);
            await _context.SaveChangesAsync();

            // Act
            var result = await _eventService.GetUpcomingEventsAsync(5);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Upcoming Event", result.First().Name);
        }
    }
}

