using EventManagementSystem.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Contracts
{
    public interface ITicketService
    {
        Task CreateTicketAsync(int eventId, string userId, DateTime purchaseDate);
        Task<List<Ticket>> GetUserTicketsAsync(string userId);
    }
}
