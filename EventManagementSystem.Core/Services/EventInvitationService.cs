using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Infrastructure.Data.Entities;
using EventManagementSystem.Infrastructure.Data.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Services
{

    public class EventInvitationService : IEventInvitationService
    {
        private readonly EventDbContext _context;

        public EventInvitationService(EventDbContext context)
        {
            _context = context;
        }



        public async Task SendInvitationAsync(string senderId, string receiverId, int eventId)
        {
            var invitation = new EventInvitation
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                EventId = eventId,
                InvitationDate = DateTime.UtcNow,
                Status = InvitationStatus.Pending
            };

            _context.EventInvitations.Add(invitation);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<EventInvitation>> GetPendingInvitationsAsync(string userId)
        {
            return await _context.EventInvitations
                .Where(i => i.ReceiverId == userId && i.Status == InvitationStatus.Pending)
                .ToListAsync();
        }

        public async Task<EventInvitation> GetInvitationByIdAsync(int id)
        {
            var invitation = await _context.EventInvitations
                .Include(i => i.Event)
                .Include(i => i.Sender)
                .Include(i => i.Receiver)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invitation == null)
            {
                // Optionally throw an exception or return null to indicate not found
                throw new KeyNotFoundException($"Invitation with ID {id} not found.");
            }

            return invitation;
        }

        public async Task DeleteInvitationAsync(int id)
        {
            var invitation = await _context.EventInvitations.FindAsync(id);
            if (invitation != null)
            {
                _context.EventInvitations.Remove(invitation);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RespondToInvitationAsync(int invitationId, bool accept)
        {
            var invitation = await _context.EventInvitations.FindAsync(invitationId);
            if (invitation == null) return;

            invitation.Status = accept ? InvitationStatus.Accepted : InvitationStatus.Declined;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<EventInvitation>> GetInvitationsForEventAsync(int eventId)
        {
            return await _context.EventInvitations
                .Where(i => i.EventId == eventId).ToListAsync();
        }

        public async Task<IEnumerable<EventInvitation>> GetAllInvitationsForUserAsync(string userId)
        {
            return await _context.EventInvitations
        .Where(i => i.SenderId == userId || i.ReceiverId == userId)
        .Include(i => i.Event)
        .Include(i => i.Sender)
        .Include(i => i.Receiver)
        .ToListAsync();
        }

        public async Task ConfirmInvitationAsync(int invitationId)
        {
            var invitation = await _context.EventInvitations.FindAsync(invitationId);
            if (invitation == null)
            {
                throw new KeyNotFoundException("Invitation not found");
            }

            invitation.Status = InvitationStatus.Accepted;
            await _context.SaveChangesAsync();

           
        }
    }
}

