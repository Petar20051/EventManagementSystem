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
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ServicesTests
{
    public class EventServiceTests
    {

        private readonly DbContextOptions<EventDbContext> _options;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

        public EventServiceTests()
        {
            _options = new DbContextOptionsBuilder<EventDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _mockNotificationService = new Mock<INotificationService>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);
        }

        private Core.Services.EventService CreateService()
        {
            var dbContext = new EventDbContext(_options);
            return new Core.Services.EventService(dbContext, _mockUserManager.Object, _mockNotificationService.Object);
        }

        private async Task SeedDataAsync(EventDbContext dbContext)
        {
            // Add users
            var users = new List<ApplicationUser>
        {
            new ApplicationUser { Id = "user1", UserName = "Test User" },
            new ApplicationUser { Id = "user2", UserName = "Another User" }
        };
            dbContext.Users.AddRange(users);

            // Mock UserManager.Users property to return the list of users
            var usersQueryable = users.AsQueryable();
            var mockUsersDbSet = new Mock<DbSet<ApplicationUser>>();
            mockUsersDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(usersQueryable.Provider);
            mockUsersDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(usersQueryable.Expression);
            mockUsersDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(usersQueryable.ElementType);
            mockUsersDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(usersQueryable.GetEnumerator());

            _mockUserManager.Setup(um => um.Users).Returns(mockUsersDbSet.Object);

            // Add venues
            dbContext.Venues.AddRange(
                new Venue { Id = 1, Name = "Main Hall", Address = "123 Main St" },
                new Venue { Id = 2, Name = "Conference Room", Address = "456 Side Ave" }
            );

            // Add events
            dbContext.Events.AddRange(
                new Infrastructure.Entities.Event
                {
                    Id = 1,
                    Name = "Upcoming Event",
                    Date = DateTime.UtcNow.AddDays(1),
                    OrganizerId = "user1",
                    VenueId = 1,
                    TicketPrice = 50,
                    Description = "Event description"
                },
                new Infrastructure.Entities.Event
                {
                    Id = 2,
                    Name = "Past Event",
                    Date = DateTime.UtcNow.AddDays(-1),
                    OrganizerId = "user1",
                    VenueId = 2,
                    TicketPrice = 100,
                    Description = "Event description"
                }
            );

            // Add reservations
            dbContext.Reservations.Add(new Reservation
            {
                Id = 1,
                EventId = 1,
                UserId = "user1",
                AttendeesCount = 2,
                IsPaid = true
            });

            await dbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetUpcomingEventsAsync_ReturnsUpcomingEvents()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);

            var service = CreateService();

            var events = await service.GetUpcomingEventsAsync();

            Assert.Single(events);
            Assert.Equal("Upcoming Event", events.First().Name);
        }

        

        

        [Fact]
        public async Task GetAllEventsAsync_ReturnsAllEvents()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);

            var service = CreateService();

            var events = await service.GetAllEventsAsync();

            Assert.Equal(2, events.Count);
        }

        [Fact]
        public async Task GetEventByIdAsync_ReturnsCorrectEvent()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);

            var service = CreateService();

            var eventDetails = await service.GetEventByIdAsync(1);

            Assert.NotNull(eventDetails);
            Assert.Equal("Upcoming Event", eventDetails.Name);
        }

        [Fact]
        public async Task UpdateEventAsync_UpdatesEventDetails()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);

            var service = CreateService();
            var eventToUpdate = await dbContext.Events.FindAsync(1);
            eventToUpdate.Name = "Updated Event";

            await service.UpdateEventAsync(eventToUpdate);

            Assert.Equal("Updated Event", (await dbContext.Events.FindAsync(1)).Name);
        }

        [Fact]
        public async Task HasTicketsAsync_ReturnsCorrectValue()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);

            var service = CreateService();

            var hasTickets = await service.HasTicketsAsync(1);

            Assert.False(hasTickets);
        }

        

        [Fact]
        public async Task SearchEventsAsync_FiltersEventsCorrectly()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);

            var service = CreateService();

            var searchResults = await service.SearchEventsAsync("Upcoming", null, null, null, null, null, null);

            Assert.Single(searchResults);
            Assert.Equal("Upcoming Event", searchResults.First().Name);
        }

        [Fact]
        public async Task GetAllAvailableEventsAsync_ReturnsEventsWithDetails()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);

            var service = CreateService();

            var events = await service.GetAllAvailableEventsAsync();

            Assert.Equal(2, events.Count);
        }

        [Fact]
        public async Task GetEventDetailsAsync_ReturnsEventDetails()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);

            var service = CreateService();

            var eventDetails = await service.GetEventDetailsAsync(1);

            Assert.NotNull(eventDetails);
            Assert.Equal(1, eventDetails.Id);
        }
    }
}

