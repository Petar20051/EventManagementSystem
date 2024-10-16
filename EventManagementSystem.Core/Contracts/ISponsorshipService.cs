using EventManagementSystem.Infrastructure.Data.Enums;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Contracts
{
    public interface ISponsorshipService
    {
        Task ProcessSponsorshipAsync(ApplicationUser user, decimal amount, string selectedCardId);
        Task UpdateSponsorshipTierAsync(ApplicationUser user, UserManager<ApplicationUser> userManager);
    }
}
