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
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }

            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<string> GetStripeCustomerIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }

            var user = await _userManager.FindByIdAsync(userId);

            // Return the StripeCustomerId or null if the user is not found.
            return user?.StripeCustomerId;
        }

        public async Task SaveStripeCustomerIdAsync(string userId, string stripeCustomerId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }

            if (string.IsNullOrEmpty(stripeCustomerId))
            {
                throw new ArgumentException("Stripe Customer ID cannot be null or empty.", nameof(stripeCustomerId));
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            user.StripeCustomerId = stripeCustomerId;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Failed to update the user's Stripe customer ID.");
            }
        }
    }
}
