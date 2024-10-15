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
    public class VenueService : IVenueService
    {
        private readonly EventDbContext _context;

        public VenueService(EventDbContext context)
        {
            _context = context;
        }

        public async Task<List<Venue>> GetAllVenuesAsync()
        {
            return await _context.Venues.ToListAsync();
        }
        public async Task<Venue> GetVenueByIdAsync(int venueId)
        {
            return await _context.Venues.FirstOrDefaultAsync(v => v.Id == venueId);
        }
    }

}
