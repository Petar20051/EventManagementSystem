using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Models.Events
{
    public class EventDetailsViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime Date { get; set; }

        public string? Location { get; set; }
        public string? OrganizerId { get; set; }
        public int? VenueId { get; set; }

    }
}
