using EventManagementSystem.Core.Models.Venue;
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
        public DateTime Date { get; set; }
        public int? VenueId { get; set; }
        public List<VenueViewModel>? Venues { get; set; }
    }
}
