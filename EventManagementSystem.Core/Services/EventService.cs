using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Infrastructure.Entities;
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

        public EventService(EventDbContext context)
        {
            _context = context;
        }

        public async Task AddEventAsync(Event events)
        {
            _context.Events.Add(events);
        await _context.SaveChangesAsync();
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
            return await _context.Events.ToListAsync();
        }

        public async Task<Event> GetEventByIdAsync(int id)
        {
            var eventbyid= await _context.Events.FindAsync(id);
            return eventbyid;
        }

        public async Task UpdateEventAsync(Event events)
        {
            _context.Events.Update(events);
            await _context.SaveChangesAsync();
        }
    }
}
