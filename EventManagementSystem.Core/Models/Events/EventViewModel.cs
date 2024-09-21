using EventManagementSystem.Core.Models.Venue;
using EventManagementSystem.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Models.Events
{
    public class EventViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        
        public string OrganizerId {  get; set; }
        public int VenueId {  get; set; }
        // Add other properties as needed

        public List<VenueViewModel> Venues { get; set; }
    }
}
