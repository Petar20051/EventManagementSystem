using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Events;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Services
{
    public class AttendeeService : IAttendeeService
    {
        private readonly EventDbContext _context;

        public AttendeeService(EventDbContext context)
        {
            _context = context;
        }

        public async Task<List<AttendeeViewModel>> GetAttendeesForEventAsync(int eventId, string organizerId)
        {
            // Verify that the event exists and that the current user is the organizer
            var @event = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId && e.OrganizerId == organizerId);



            if (@event == null)
            {
                return null; 
            }

            // Fetch attendees for the event
            var attendees = await _context.UserEvents
                .Include(ue => ue.User)
                .Where(ue => ue.EventId == eventId)
                .Select(ue => new AttendeeViewModel
                {
                    UserId = ue.UserId,
                    UserName = ue.User.UserName,
                    Email = ue.User.Email,
                   
                }).ToListAsync();

            return attendees;
        }

       
    }
}
