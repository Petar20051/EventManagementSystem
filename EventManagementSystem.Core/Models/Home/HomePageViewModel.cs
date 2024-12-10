using EventManagementSystem.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Models.Home
{
    public class HomePageViewModel
    {

        public List<Event> UpcomingEvents { get; set; } = new List<Event>();
        public bool IsAdmin { get; set; }

    }
}
