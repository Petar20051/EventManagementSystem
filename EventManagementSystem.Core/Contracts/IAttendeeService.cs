using EventManagementSystem.Core.Models.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Contracts
{
    public interface IAttendeeService
    {
        Task<List<AttendeeViewModel>> GetAttendeesForEventAsync(int eventId, string organizerId);
    }
}

