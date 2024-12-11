using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Models.Events
{
    public class ExtendedEventViewModel
    {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime Date { get; set; }
            public string Venue { get; set; }
            public string Address { get; set; }
            public string Description { get; set; }
           

            
            public string OrganizerEmail { get; set; }
    }
}

