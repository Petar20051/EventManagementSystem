﻿using EventMaganementSystem.Data;
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

        public async Task<List<Event>> GetUpcomingEventsAsync(int count = 5)
        {
            return await _context.Events
                                 .Where(e => e.Date >= DateTime.Now) 
                                 .OrderBy(e => e.Date) 
                                 .Take(count) 
                                 .ToListAsync();
        }

        public async Task AddEventAsync(Event events)
        {
            events.OrganizerId = events.OrganizerId ?? "default-organizer-id";  
            _context.Events.Add(events);
            await _context.SaveChangesAsync();

            
            var users = await _userManager.Users.ToListAsync();
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

            
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e => e.Name.Contains(searchTerm) || e.Description.Contains(searchTerm));
            }

            
            if (startDate.HasValue)
            {
                query = query.Where(e => e.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.Date <= endDate.Value);
            }

            
            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(e => e.Venue.Address.Contains(location));
            }

            
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
