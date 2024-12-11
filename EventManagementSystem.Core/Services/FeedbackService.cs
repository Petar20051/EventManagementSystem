using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Services
{
    public class FeedbackService:IFeedbackService
    {
        private readonly EventDbContext _context;
        public FeedbackService(EventDbContext context)
        {
            _context = context;
        }
        public async Task AddFeedbackAsync(Feedback feedback)
        {
            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Feedback>> GetFeedbacksByEventIdAsync(int eventId)
        {
            return await _context.Feedbacks
         .Include(f => f.User) 
         .Where(f => f.EventId == eventId)
         .OrderByDescending(f => f.FeedbackDate)
         .ToListAsync();
        }
    }
}
