using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Contracts
{
    public interface IProfileService
    {
       
            Task<ApplicationUser> GetUserAsync(string userId);
            Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword);
       
    }
}
