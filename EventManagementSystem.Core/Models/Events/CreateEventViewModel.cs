using EventManagementSystem.Core.Models.Venue;
using EventManagementSystem.Infrastructure.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Models.Events
{
    public class CreateEventViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public EventTypes EventType { get; set; }
        public DateTime Date { get; set; }
        public int? VenueId { get; set; }
        public List<VenueViewModel>? Venues { get; set; } = new List<VenueViewModel>();

        public decimal TicketPrice { get; set; }

        public string ImageURL { get; set; }
    }
}
