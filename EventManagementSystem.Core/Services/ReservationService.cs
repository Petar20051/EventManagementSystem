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
            
            var eventEntity = await _context.Events
                .Include(e => e.Venue) 
                .FirstOrDefaultAsync(e => e.Id == reservation.EventId);

            
            if (eventEntity == null)
            {
                throw new InvalidOperationException("Event not found.");
            }

            
            if (eventEntity.Venue.Capacity < reservation.AttendeesCount)
            {
                throw new InvalidOperationException("Venue is fully booked.");
            }

            
            var existingReservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.EventId == reservation.EventId && r.UserId == reservation.UserId);

            if (existingReservation != null)
            {
                throw new InvalidOperationException("You already have a reservation for this event.");
            }

            
            if (reservation.TotalAmount == 0 && eventEntity.TicketPrice > 0)
            {
                reservation.TotalAmount = eventEntity.TicketPrice * reservation.AttendeesCount;
            }

            
            _context.Reservations.Add(reservation);
            eventEntity.Venue.Capacity -= reservation.AttendeesCount;

            await _context.SaveChangesAsync();
            return reservation;
        }

        
        public async Task<Reservation> GetReservationByIdAsync(int id)
            {
                return await _context.Reservations
                    .Include(r => r.Event) 
                    .Include(r => r.Event.Venue) 
                    .FirstOrDefaultAsync(r => r.Id == id);
            }

            
            public async Task<IEnumerable<Reservation>> GetAllReservationsAsync()
            {
                return await _context.Reservations
                    .Include(r => r.Event) 
                    .Include(r => r.Event.Venue) 
                    .ToListAsync();
            }

            
            public async Task<Reservation> UpdateReservationAsync(Reservation updatedReservation)
            {
               
                var existingReservation = await _context.Reservations.FindAsync(updatedReservation.Id);

                if (existingReservation == null)
                {
                    throw new InvalidOperationException("Reservation not found.");
                }

                
                existingReservation.IsPaid = updatedReservation.IsPaid;
                existingReservation.PaymentDate = updatedReservation.PaymentDate;

                await _context.SaveChangesAsync(); 

                return existingReservation; 
            }

            
            public async Task<bool> DeleteReservationAsync(int id)
            {
                var reservation = await _context.Reservations.FindAsync(id);
                if (reservation == null) return false;

                _context.Reservations.Remove(reservation);

                
                var eventEntity = await _context.Events.Include(e => e.Venue).FirstOrDefaultAsync(e => e.Id == reservation.EventId);
                if (eventEntity != null)
                {
                    eventEntity.Venue.Capacity += reservation.AttendeesCount;
                }

                await _context.SaveChangesAsync();
                return true;
            }

            
            public async Task<IEnumerable<Reservation>> GetReservationsByEventIdAsync(int eventId)
            {
                return await _context.Reservations
                    .Where(r => r.EventId == eventId)
                    .Include(r => r.Event) 
                    .Include(r => r.Event.Venue) 
                    .ToListAsync();
            }
        }



    }

