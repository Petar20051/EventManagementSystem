using EventManagementSystem.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Contracts
{
    public interface IUserEventService
    {
        Task AddUserEventAsync(UserEvent userEvent);
    }
}
