using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Services
{
    public class UserEventService : IUserEventService
    {
        private readonly EventDbContext _context;

        public UserEventService(EventDbContext context)
        {
            _context = context;
        }

        public async Task AddUserEventAsync(string userId, int eventId)
        {
            if (string.IsNullOrEmpty(userId))  
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }

            if (eventId <= 0) 
            {
                throw new ArgumentException("Event ID must be a valid positive integer.", nameof(eventId));
            }

            var eventEntity = await _context.Events.FindAsync(eventId);
            if (eventEntity == null)
            {
                throw new InvalidOperationException("Event not found.");
            }

            var userEvent = new UserEvent
            {
                UserId = userId,
                EventId = eventId
            };

            _context.UserEvents.Add(userEvent);
            await _context.SaveChangesAsync();
        }
    }
}
