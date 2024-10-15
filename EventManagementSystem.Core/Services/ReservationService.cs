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
    public class ReservationService : IReservationService
    {
        private readonly EventDbContext _context;

        public ReservationService(EventDbContext context)
        {
            _context = context;
        }

        // Create a new reservation
        public async Task<Reservation> CreateReservationAsync(Reservation reservation)
        {
            // Fetch event with its associated venue
            var eventEntity = await _context.Events
                .Include(e => e.Venue) // Include venue to access capacity
                .FirstOrDefaultAsync(e => e.Id == reservation.EventId);

            if (eventEntity == null)
            {
                throw new InvalidOperationException("Event not found.");
            }

            // Check venue capacity for the event
            if (eventEntity.Venue.Capacity <= 0)
            {
                throw new InvalidOperationException("Venue is fully booked.");
            }

            // Check if the user already has a reservation for this event
            var existingReservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.EventId == reservation.EventId && r.UserId == reservation.UserId);

            if (existingReservation != null)
            {
                throw new InvalidOperationException("You already have a reservation for this event.");
            }

            // Create the reservation and update venue capacity
            _context.Reservations.Add(reservation);
            eventEntity.Venue.Capacity=eventEntity.Venue.Capacity-reservation.AttendeesCount;

            await _context.SaveChangesAsync();
            return reservation;
        }

        // Get a reservation by ID
        public async Task<Reservation> GetReservationByIdAsync(int id)
        {
            return await _context.Reservations.FindAsync(id);
        }

        // Get all reservations
        public async Task<IEnumerable<Reservation>> GetAllReservationsAsync()
        {
            return await _context.Reservations
                .Include(r => r.Event) // Include event details in the reservations
                .Include(r => r.Event.Venue) // Include venue details of events
                .ToListAsync();
        }

        // Update an existing reservation
        public async Task<Reservation> UpdateReservationAsync(Reservation updatedReservation)
        {
            // Find the existing reservation by Id
            var existingReservation = await _context.Reservations.FindAsync(updatedReservation.Id);

            if (existingReservation == null)
            {
                throw new InvalidOperationException("Reservation not found.");
            }

            // Update only the necessary fields
            existingReservation.IsPaid = updatedReservation.IsPaid;  // Update payment status
            existingReservation.PaymentDate = updatedReservation.PaymentDate; // Optionally update payment date

            // You can update other fields as needed, but here we're just focusing on payment status
            // e.g., existingReservation.TotalAmount, etc., can remain unchanged

            await _context.SaveChangesAsync();  // Persist changes to the database

            return existingReservation;  // Return the updated reservation
        }

        // Delete a reservation
        public async Task<bool> DeleteReservationAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null) return false;

            _context.Reservations.Remove(reservation);

            // Increase venue capacity when reservation is removed
            var eventEntity = await _context.Events.Include(e => e.Venue).FirstOrDefaultAsync(e => e.Id == reservation.EventId);
            if (eventEntity != null)
            {
                eventEntity.Venue.Capacity++;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Get reservations by event ID
        public async Task<IEnumerable<Reservation>> GetReservationsByEventIdAsync(int eventId)
        {
            return await _context.Reservations
                .Where(r => r.EventId == eventId)
                .Include(r => r.Event) // Include event details
                .Include(r => r.Event.Venue) // Include venue details
                .ToListAsync();
        }
    }


}
