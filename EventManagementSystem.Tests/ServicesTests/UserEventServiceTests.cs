using EventMaganementSystem.Data;
using EventManagementSystem.Core.Services;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ServicesTests
{
    public class UserEventServiceTests
    {
        private readonly UserEventService _userEventService;
        private readonly EventDbContext _context;

        public UserEventServiceTests()
        {
            // Setup InMemory DbContext for testing
            var options = new DbContextOptionsBuilder<EventDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Unique database for each test
                .Options;

            _context = new EventDbContext(options);
            _userEventService = new UserEventService(_context);
        }

        [Fact]
        public async Task AddUserEventAsync_ShouldAddUserEventSuccessfully()
        {
            // Arrange
            var userId = "test-user-id";
            var eventId = 1;

            // Ensure the event exists in the database with all required fields
            var eventEntity = new Event
            {
                Id = eventId,
                Name = "Test Event",
                Description = "This is a test event",  // Add required Description
                TicketPrice = 50.00m,
                OrganizerId = "organizer1"  // Add required OrganizerId
            };
            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();

            // Act
            await _userEventService.AddUserEventAsync(userId, eventId);

            // Assert
            var userEvent = await _context.UserEvents
                                           .FirstOrDefaultAsync(ue => ue.UserId == userId && ue.EventId == eventId);
            Assert.NotNull(userEvent);
            Assert.Equal(userId, userEvent.UserId);
            Assert.Equal(eventId, userEvent.EventId);
        }


        [Fact]
        public async Task AddUserEventAsync_ShouldNotAddUserEvent_WhenEventDoesNotExist()
        {
            // Arrange
            var userId = "test-user-id";
            var nonExistentEventId = 999;  // ID for an event that doesn't exist in the database

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userEventService.AddUserEventAsync(userId, nonExistentEventId));

            // Assert that the exception message is correct
            Assert.Equal("Event not found.", exception.Message);
        }

        [Fact]
        public async Task AddUserEventAsync_ShouldThrowException_WhenUserIdIsNullOrEmpty()
        {
            // Arrange
            var invalidUserId = ""; // Empty userId
            var eventId = 1;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _userEventService.AddUserEventAsync(invalidUserId, eventId));

            Assert.Equal("User ID cannot be null or empty. (Parameter 'userId')", exception.Message);
        }

        [Fact]
        public async Task AddUserEventAsync_ShouldThrowException_WhenEventIdIsInvalid()
        {
            // Arrange
            var userId = "test-user-id";
            var invalidEventId = 0; // Invalid eventId (can also test with -1)

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _userEventService.AddUserEventAsync(userId, invalidEventId));

            Assert.Equal("Event ID must be a valid positive integer. (Parameter 'eventId')", exception.Message);
        }
    }
}
