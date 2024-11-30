using EventMaganementSystem.Data;
using EventManagementSystem.Core.Services;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ServicesTests
{
    public class ReservationServiceTests
    {
        private readonly EventDbContext _context;
        private readonly ReservationService _reservationService;

        public ReservationServiceTests()
        {
            var options = new DbContextOptionsBuilder<EventDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new EventDbContext(options);
            _reservationService = new ReservationService(_context);
        }

        [Fact]
        public async Task CreateReservationAsync_ShouldCreateReservation_WhenValid()
        {
            // Arrange
            var venue = new Venue
            {
                Id = 1,
                Name = "Venue 1",
                Address = "123 Main Street", // Required property
                Capacity = 100
            };
            var eventEntity = new Event
            {
                Id = 1,
                Name = "Test Event",
                VenueId = venue.Id,
                Venue = venue, // Navigation property
                TicketPrice = 50.00m // Assuming TicketPrice is required
            };

            _context.Venues.Add(venue);
            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();

            var reservation = new Reservation
            {
                Id = 1,
                EventId = 1, // Link to Event
                UserId = "user1", // User making the reservation
                AttendeesCount = 5
            };

            // Act
            var result = await _reservationService.CreateReservationAsync(reservation);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(reservation.Id, result.Id);
            Assert.Equal(95, venue.Capacity); // Venue capacity should decrease
            Assert.Equal(250.00m, reservation.TotalAmount); // 50 * 5 = 250
        }


        [Fact]
        public async Task CreateReservationAsync_ShouldThrowException_WhenEventNotFound()
        {
            // Arrange
            var reservation = new Reservation { Id = 1, EventId = 999, UserId = "user1" };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _reservationService.CreateReservationAsync(reservation));
        }

        [Fact]
        public async Task CreateReservationAsync_ShouldThrowException_WhenVenueIsFullyBooked()
        {
            // Arrange
            var venue = new Venue { Id = 1, Capacity = 0 };
            var eventEntity = new Event { Id = 1, Name = "Test Event", Venue = venue };
            _context.Venues.Add(venue);
            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();

            var reservation = new Reservation { Id = 1, EventId = 1, UserId = "user1" };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _reservationService.CreateReservationAsync(reservation));
        }

        [Fact]
        public async Task CreateReservationAsync_ShouldThrowException_WhenUserAlreadyHasReservation()
        {
            // Arrange
            var venue = new Venue { Id = 1, Capacity = 100 };
            var eventEntity = new Event { Id = 1, Name = "Test Event", Venue = venue };
            _context.Venues.Add(venue);
            _context.Events.Add(eventEntity);
            var existingReservation = new Reservation { Id = 1, EventId = 1, UserId = "user1" };
            _context.Reservations.Add(existingReservation);
            await _context.SaveChangesAsync();

            var newReservation = new Reservation { Id = 2, EventId = 1, UserId = "user1" };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _reservationService.CreateReservationAsync(newReservation));
        }

        [Fact]
        public async Task GetReservationByIdAsync_ShouldReturnReservation_WhenExists()
        {
            // Arrange
            var reservation = new Reservation { Id = 1, EventId = 1, UserId = "user1" };
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _reservationService.GetReservationByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetReservationByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            // Act
            var result = await _reservationService.GetReservationByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllReservationsAsync_ShouldReturnAllReservations()
        {
            // Arrange
            var reservation1 = new Reservation { Id = 1, EventId = 1, UserId = "user1" };
            var reservation2 = new Reservation { Id = 2, EventId = 2, UserId = "user2" };
            _context.Reservations.AddRange(reservation1, reservation2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _reservationService.GetAllReservationsAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task UpdateReservationAsync_ShouldUpdateReservation_WhenValid()
        {
            // Arrange
            var reservation = new Reservation { Id = 1, EventId = 1, UserId = "user1", IsPaid = false };
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            var updatedReservation = new Reservation { Id = 1, IsPaid = true, PaymentDate = DateTime.Now };

            // Act
            var result = await _reservationService.UpdateReservationAsync(updatedReservation);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsPaid);
            Assert.NotNull(result.PaymentDate);
        }

        [Fact]
        public async Task UpdateReservationAsync_ShouldThrowException_WhenReservationNotFound()
        {
            // Arrange
            var updatedReservation = new Reservation { Id = 999, IsPaid = true };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _reservationService.UpdateReservationAsync(updatedReservation));
        }

        [Fact]
        public async Task DeleteReservationAsync_ShouldDeleteReservation_WhenExists()
        {
            // Arrange
            var venue = new Venue { Id = 1, Capacity = 50 };
            var eventEntity = new Event { Id = 1, Venue = venue };
            var reservation = new Reservation { Id = 1, EventId = 1, UserId = "user1" };
            _context.Venues.Add(venue);
            _context.Events.Add(eventEntity);
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _reservationService.DeleteReservationAsync(1);

            // Assert
            Assert.True(result);
            Assert.Null(await _context.Reservations.FindAsync(1));
            Assert.Equal(51, venue.Capacity); // Capacity should increase
        }

        [Fact]
        public async Task DeleteReservationAsync_ShouldReturnFalse_WhenReservationNotFound()
        {
            // Act
            var result = await _reservationService.DeleteReservationAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetReservationsByEventIdAsync_ShouldReturnReservationsForEvent()
        {
            // Arrange
            var venue = new Venue
            {
                Id = 1,
                Name = "Venue 1",
                Address = "123 Main Street", // Required property
                Capacity = 100
            };
            var event1 = new Event
            {
                Id = 1,
                Name = "Event 1",
                VenueId = venue.Id, // Link to Venue
                Venue = venue // Navigation property
            };
            var reservation1 = new Reservation
            {
                Id = 1,
                EventId = 1, // Link to Event
                UserId = "user1" // User making the reservation
            };

            // Add entities to the InMemory database
            _context.Venues.Add(venue);
            _context.Events.Add(event1);
            _context.Reservations.Add(reservation1);
            await _context.SaveChangesAsync();

            // Act
            var result = await _reservationService.GetReservationsByEventIdAsync(1);

            // Assert
            Assert.Single(result);
            Assert.Equal("user1", result.First().UserId);
            Assert.Equal("Event 1", result.First().Event.Name);
            Assert.Equal("Venue 1", result.First().Event.Venue.Name);
        }

    }
}
