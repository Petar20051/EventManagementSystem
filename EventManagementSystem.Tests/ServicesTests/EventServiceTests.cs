using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Services;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        private readonly EventDbContext _context;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly EventService _eventService;

        public EventServiceTests()
        {
            var options = new DbContextOptionsBuilder<EventDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new EventDbContext(options);

            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null
            );

            _mockNotificationService = new Mock<INotificationService>();

            _eventService = new EventService(_context, _mockUserManager.Object, _mockNotificationService.Object);
        }

        [Fact]
        public async Task GetAllAvailableEventsAsync_ShouldReturnAllEventsWithDetails()
        {
            // Arrange
            _context.Events.Add(new Event { Id = 1, Name = "Event 1" });
            _context.Events.Add(new Event { Id = 2, Name = "Event 2" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _eventService.GetAllAvailableEventsAsync();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetEventDetailsAsync_ShouldReturnEventById()
        {
            // Arrange
            var event1 = new Event { Id = 1, Name = "Event 1" };
            _context.Events.Add(event1);
            await _context.SaveChangesAsync();

            // Act
            var result = await _eventService.GetEventDetailsAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Event 1", result.Name);
        }

        [Fact]
        public async Task SearchEventsAsync_ShouldFilterBySearchTermAndDate()
        {
            // Arrange
            var event1 = new Event { Id = 1, Name = "Music Fest", Date = DateTime.Now.AddDays(1), Description = "Great Music" };
            var event2 = new Event { Id = 2, Name = "Tech Talk", Date = DateTime.Now.AddDays(5), Description = "Tech Trends" };

            _context.Events.AddRange(event1, event2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _eventService.SearchEventsAsync("Music", DateTime.Now, null, null, null, null, null);

            // Assert
            Assert.Single(result);
            Assert.Equal("Music Fest", result.First().Name);
        }

        [Fact]
        public async Task GetAttendeesForEventAsync_ShouldReturnAttendeeInfo()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user1", UserName = "user@example.com" };
            var event1 = new Event { Id = 1, Name = "Event 1" };
            var reservation = new Reservation { Id = 1, EventId = 1, User = user, IsPaid = true, AttendeesCount = 2 };

            _context.Users.Add(user);
            _context.Events.Add(event1);
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Act
            var attendees = await _eventService.GetAttendeesForEventAsync(1);

            // Assert
            Assert.Single(attendees);
            Assert.Equal("user@example.com", attendees.First().UserEmail);
            Assert.Equal(2, attendees.First().AttendeeCount);
        }

        [Fact]
        public async Task AddEventAsync_ShouldAddEventAndSendNotifications()
        {
            // Arrange
            var newEvent = new Event { Id = 1, Name = "New Event", Date = DateTime.Now.AddDays(5) };
            _mockUserManager.Setup(um => um.Users).Returns(new List<ApplicationUser>
        {
            new ApplicationUser { Id = "user1", UserName = "User1" },
            new ApplicationUser { Id = "user2", UserName = "User2" }
        }.AsQueryable());

            // Act
            await _eventService.AddEventAsync(newEvent);

            // Assert
            var addedEvent = await _context.Events.FindAsync(newEvent.Id);
            Assert.NotNull(addedEvent);
            _mockNotificationService.Verify(ns => ns.NotifyNewEventAsync("user1", "New Event"), Times.Once);
            _mockNotificationService.Verify(ns => ns.NotifyNewEventAsync("user2", "New Event"), Times.Once);
        }

        [Fact]
        public async Task HasTicketsAsync_ShouldReturnFalseIfNoTickets()
        {
            // Arrange
            var event1 = new Event { Id = 1, Name = "Event 1" };
            _context.Events.Add(event1);
            await _context.SaveChangesAsync();

            // Act
            var result = await _eventService.HasTicketsAsync(1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateEventAsync_ShouldUpdateEvent()
        {
            // Arrange
            var event1 = new Event { Id = 1, Name = "Old Event" };
            _context.Events.Add(event1);
            await _context.SaveChangesAsync();

            // Act
            event1.Name = "Updated Event";
            await _eventService.UpdateEventAsync(event1);

            // Assert
            var updatedEvent = await _context.Events.FindAsync(1);
            Assert.NotNull(updatedEvent);
            Assert.Equal("Updated Event", updatedEvent.Name);
        }
    }
    }
