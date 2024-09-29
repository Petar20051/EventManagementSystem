using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Models.Events
{
    
        public class AttendeeViewModel
        {
            public string UserId { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
            public DateTime ReservationDate { get; set; }
        }
    

}
