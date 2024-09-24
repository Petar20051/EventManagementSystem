using EventManagementSystem.Infrastructure.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Contracts
{
    public interface IEventInvitationService
    {
        Task SendInvitationAsync(string senderId, string receiverId, int eventId);
        Task<IEnumerable<EventInvitation>> GetPendingInvitationsAsync(string userId);
        Task RespondToInvitationAsync(int invitationId, bool accept);
        Task<IEnumerable<EventInvitation>> GetInvitationsForEventAsync(int eventId);
        Task<EventInvitation> GetInvitationByIdAsync(int id); 
        Task DeleteInvitationAsync(int id);
        Task<IEnumerable<EventInvitation>> GetAllInvitationsForUserAsync(string userId);
        Task ConfirmInvitationAsync(int invitationId);
        
    }
}
