using EventManagementSystem.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Contracts
{
    public interface IUserService
    {
        Task<string> GetStripeCustomerIdAsync(string userId);
        Task SaveStripeCustomerIdAsync(string userId, string customerId);
        Task<ApplicationUser> GetUserByIdAsync(string userId);
    }
}
