using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Events;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Services
{
    public class EventService : IEventService
    {
        private readonly EventDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        public EventService(EventDbContext context, UserManager<ApplicationUser> userManager, INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task AddEventAsync(Event events)
        {
            // Add the new event to the database
            _context.Events.Add(events);
            await _context.SaveChangesAsync();

            // Retrieve all users to send the notification
            var users = await _userManager.Users.ToListAsync();

            // Notify each user about the new event
            foreach (var user in users)
            {
                await _notificationService.NotifyNewEventAsync(user.Id, events.Name);
            }
        }

        public async Task DeleteEventAsync(int id)
        {
            var eventToDelete = await _context.Events.FindAsync(id);
            if (eventToDelete != null)
            {
                _context.Events.Remove(eventToDelete);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Event>> GetAllEventsAsync()
        {
            return await _context.Events.Include(e => e.Venue).ToListAsync();
        }

        public async Task<Event> GetEventByIdAsync(int id)
        {
            var eventbyid = await _context.Events.FindAsync(id);
            return eventbyid;
        }

        public async Task UpdateEventAsync(Event events)
        {
            _context.Events.Update(events);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasTicketsAsync(int eventId)
        {
            return await _context.Tickets.AnyAsync(t => t.EventId == eventId);
        }

        public async Task<List<AttendeeInfo>> GetAttendeesForEventAsync(int eventId)
        {
            var reservations = await _context.Reservations.Where(r => r.EventId == eventId && r.IsPaid == true).ToListAsync();
            var users = reservations.Select(r => new AttendeeInfo
            {
                UserEmail = r.User.UserName,
                AttendeeCount = r.AttendeesCount
            }
                ).ToList();
            return users;

        }
        public async Task<List<Event>> SearchEventsAsync(string searchTerm, DateTime? startDate, DateTime? endDate, string location, string eventType, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Events.Include(e => e.Venue).AsQueryable();

            // Search by term (event name or description)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e => e.Name.Contains(searchTerm) || e.Description.Contains(searchTerm));
            }

            // Filter by date range
            if (startDate.HasValue)
            {
                query = query.Where(e => e.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.Date <= endDate.Value);
            }

            // Filter by location
            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(e => e.Venue.Address.Contains(location));
            }

            // Filter by event type
            //if (!string.IsNullOrEmpty(eventType))
            //{
            //    query = query.Where(e => e.Type.Name == eventType);
            //}

            // Filter by price range
            if (minPrice.HasValue)
            {
                query = query.Where(e => e.TicketPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(e => e.TicketPrice <= maxPrice.Value);
            }

            return await query.ToListAsync();
        }
        public async Task<List<Event>> GetAllAvailableEventsAsync()
        {
            return await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Organizer)
                .ToListAsync();
        }

        public async Task<Event> GetEventDetailsAsync(int eventId)
        {
            return await _context.Events
                .Where(e => e.Id == eventId)
                .FirstOrDefaultAsync();
        }
    }
}
