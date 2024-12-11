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
            
            var options = new DbContextOptionsBuilder<EventDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) 
                .Options;

            _context = new EventDbContext(options);
            _userEventService = new UserEventService(_context);
        }

        [Fact]
        public async Task AddUserEventAsync_ShouldAddUserEventSuccessfully()
        {
            
            var userId = "test-user-id";
            var eventId = 1;

            
            var eventEntity = new Event
            {
                Id = eventId,
                Name = "Test Event",
                Description = "This is a test event",  
                TicketPrice = 50.00m,
                OrganizerId = "organizer1"  
            };
            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();

            
            await _userEventService.AddUserEventAsync(userId, eventId);

            
            var userEvent = await _context.UserEvents
                                           .FirstOrDefaultAsync(ue => ue.UserId == userId && ue.EventId == eventId);
            Assert.NotNull(userEvent);
            Assert.Equal(userId, userEvent.UserId);
            Assert.Equal(eventId, userEvent.EventId);
        }


        [Fact]
        public async Task AddUserEventAsync_ShouldNotAddUserEvent_WhenEventDoesNotExist()
        {
            
            var userId = "test-user-id";
            var nonExistentEventId = 999;  

            
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userEventService.AddUserEventAsync(userId, nonExistentEventId));

            
            Assert.Equal("Event not found.", exception.Message);
        }

        [Fact]
        public async Task AddUserEventAsync_ShouldThrowException_WhenUserIdIsNullOrEmpty()
        {
            
            var invalidUserId = ""; 
            var eventId = 1;

            
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _userEventService.AddUserEventAsync(invalidUserId, eventId));

            Assert.Equal("User ID cannot be null or empty. (Parameter 'userId')", exception.Message);
        }

        [Fact]
        public async Task AddUserEventAsync_ShouldThrowException_WhenEventIdIsInvalid()
        {
            
            var userId = "test-user-id";
            var invalidEventId = 0; 

            
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _userEventService.AddUserEventAsync(userId, invalidEventId));

            Assert.Equal("Event ID must be a valid positive integer. (Parameter 'eventId')", exception.Message);
        }
    }
}
