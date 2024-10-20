﻿using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using PayPalCheckoutSdk.Orders;
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
            // Create the ticket
           

            var ticket = new Ticket
            {
                EventId = eventId,
                HolderId = userId,
                PurchaseDate = purchaseDate
            };

            // Add the ticket to the database
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Ticket>> GetUserTicketsAsync(string userId)
        {
            // Retrieve tickets for the logged-in user, including Event and Reservations
            return await _context.Tickets
                .Include(t => t.Event)
                .ThenInclude(e => e.Reservations) // Include related reservations for the event
                .Where(t => t.HolderId == userId)
                .ToListAsync();
        }
    }
}
