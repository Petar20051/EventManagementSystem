using EventManagementSystem.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Contracts
{
    public interface IEventService
    {
    Task AddEventAsync(Event events);

    Task<Event> GetEventByIdAsync(int id);
    Task UpdateEventAsync(Event events);

    Task DeleteEventAsync(int id);
    Task<List<Event>> GetAllEventsAsync();

    }
}
