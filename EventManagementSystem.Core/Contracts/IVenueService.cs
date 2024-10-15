using EventManagementSystem.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Contracts
{
    public interface IVenueService
    {
        Task<List<Venue>> GetAllVenuesAsync();
        Task<Venue> GetVenueByIdAsync(int venueId);
    }
}
