using EventManagementSystem.Core.Models.Events;
using EventManagementSystem.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
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

    Task<bool> HasTicketsAsync(int eventId);

        Task<List<AttendeeInfo>> GetAttendeesForEventAsync(int eventId);

        Task<List<Event>> SearchEventsAsync(string searchTerm, DateTime? startDate, DateTime? endDate, string location, string eventType, decimal? minPrice, decimal? maxPrice);

    }
}
