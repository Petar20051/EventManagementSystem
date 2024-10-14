using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // Implement GetUserByIdAsync method
        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId); // Use UserManager to fetch user by ID
        }

        public async Task<string> GetStripeCustomerIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.StripeCustomerId;
        }

        public async Task SaveStripeCustomerIdAsync(string userId, string stripeCustomerId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.StripeCustomerId = stripeCustomerId;
                await _userManager.UpdateAsync(user); // Save the updated user
            }
        }
    }
}
