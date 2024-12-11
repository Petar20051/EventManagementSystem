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
    public class TicketServiceTests
    {
        private readonly EventDbContext _context;
        private readonly TicketService _ticketService;

        public TicketServiceTests()
        {
            var options = new DbContextOptionsBuilder<EventDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) 
                .Options;

            _context = new EventDbContext(options);
            _ticketService = new TicketService(_context);
        }

        [Fact]
        public async Task CreateTicketAsync_ShouldCreateTicketSuccessfully()
        {
            
            var eventEntity = new Event
            {
                Id = 1,
                Name = "Test Event",
                Description = "This is a test event", 
                TicketPrice = 50.00m,
                OrganizerId = "organizer1" 
            };
            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();

            var userId = "user1";
            var purchaseDate = DateTime.UtcNow;

            
            await _ticketService.CreateTicketAsync(eventEntity.Id, userId, purchaseDate);

            
            var tickets = await _context.Tickets.Include(t => t.Event).ToListAsync(); 
            Assert.Single(tickets);
            Assert.Equal(eventEntity.Id, tickets.First().EventId);
            Assert.Equal(userId, tickets.First().HolderId);
            Assert.Equal(purchaseDate, tickets.First().PurchaseDate);
            Assert.Equal("Test Event", tickets.First().Event.Name); 
        }
        [Fact]
        public async Task GetUserTicketsAsync_ShouldReturnTicketsForUser()
        {
            
            var eventEntity = new Event
            {
                Id = 1,
                Name = "Test Event",
                Description = "This is a test event", 
                TicketPrice = 50.00m,
                OrganizerId = "organizer1" 
            };
            _context.Events.Add(eventEntity);

            var ticket1 = new Ticket
            {
                Id = 1,
                EventId = eventEntity.Id,
                HolderId = "user1",
                PurchaseDate = DateTime.UtcNow
            };
            var ticket2 = new Ticket
            {
                Id = 2,
                EventId = eventEntity.Id,
                HolderId = "user2",
                PurchaseDate = DateTime.UtcNow
            };

            _context.Tickets.AddRange(ticket1, ticket2);
            await _context.SaveChangesAsync();

            
            var result = await _ticketService.GetUserTicketsAsync("user1");

            
            Assert.Single(result);
            Assert.Equal("user1", result.First().HolderId);
            Assert.Equal(eventEntity.Id, result.First().EventId);
            Assert.Equal("Test Event", result.First().Event.Name); 
        }

        [Fact]
        public async Task GetUserTicketsAsync_ShouldReturnEmptyList_WhenNoTicketsForUser()
        {
            
            var eventEntity = new Event
            {
                Id = 1,
                Name = "Test Event",
                Description = "This is a test event", 
                TicketPrice = 50.00m,
                OrganizerId = "organizer1" 
            };
            _context.Events.Add(eventEntity);

            var ticket = new Ticket
            {
                Id = 1,
                EventId = eventEntity.Id,
                HolderId = "user1", 
                PurchaseDate = DateTime.UtcNow
            };
            _context.Tickets.Add(ticket);

            await _context.SaveChangesAsync();

            
            var result = await _ticketService.GetUserTicketsAsync("user2"); 

            
            Assert.Empty(result);
        }
        [Fact]
        public async Task CreateTicketAsync_ShouldThrowException_WhenEventDoesNotExist()
        {
            
            var invalidEventId = 999; 
            var userId = "user1";
            var purchaseDate = DateTime.UtcNow;

            
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _ticketService.CreateTicketAsync(invalidEventId, userId, purchaseDate));

            Assert.Equal("Event not found.", exception.Message);
        }
    }
}
