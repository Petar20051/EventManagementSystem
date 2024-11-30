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

        public async Task<Reservation> CreateReservationAsync(Reservation reservation)
        {
            // Fetch the event along with its venue
            var eventEntity = await _context.Events
                .Include(e => e.Venue) // Ensure venue details are included
                .FirstOrDefaultAsync(e => e.Id == reservation.EventId);

            // Validate that the event exists
            if (eventEntity == null)
            {
                throw new InvalidOperationException("Event not found.");
            }

            // Validate venue capacity
            if (eventEntity.Venue.Capacity < reservation.AttendeesCount)
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

            // Set the TotalAmount if not already set (e.g., based on ticket price and attendees count)
            if (reservation.TotalAmount == 0 && eventEntity.TicketPrice > 0)
            {
                reservation.TotalAmount = eventEntity.TicketPrice * reservation.AttendeesCount;
            }

            // Add the reservation and update the venue capacity
            _context.Reservations.Add(reservation);
            eventEntity.Venue.Capacity -= reservation.AttendeesCount;

            await _context.SaveChangesAsync();
            return reservation;
        }

        // Get a reservation by ID
        public async Task<Reservation> GetReservationByIdAsync(int id)
            {
                return await _context.Reservations
                    .Include(r => r.Event) // Include event details
                    .Include(r => r.Event.Venue) // Include venue details
                    .FirstOrDefaultAsync(r => r.Id == id);
            }

            // Get all reservations
            public async Task<IEnumerable<Reservation>> GetAllReservationsAsync()
            {
                return await _context.Reservations
                    .Include(r => r.Event) // Include event details
                    .Include(r => r.Event.Venue) // Include venue details
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
                existingReservation.IsPaid = updatedReservation.IsPaid;
                existingReservation.PaymentDate = updatedReservation.PaymentDate;

                await _context.SaveChangesAsync(); // Persist changes to the database

                return existingReservation; // Return the updated reservation
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
                    eventEntity.Venue.Capacity += reservation.AttendeesCount;
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

