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
    public class TicketService:ITicketService
    {
        private readonly EventDbContext _context;

        public TicketService(EventDbContext context)
        {
            _context = context;
        }

        public async Task CreateTicketAsync(int eventId, string userId, DateTime purchaseDate)
        {
            
            var eventEntity = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);
            if (eventEntity == null)
            {
                throw new InvalidOperationException("Event not found.");
            }

            
            var ticket = new Ticket
            {
                EventId = eventId,
                HolderId = userId,
                PurchaseDate = purchaseDate
            };

            
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Ticket>> GetUserTicketsAsync(string userId)
        {
            
            return await _context.Tickets
                .Include(t => t.Event) 
                .Where(t => t.HolderId == userId)
                .ToListAsync();
        }
    }
}
