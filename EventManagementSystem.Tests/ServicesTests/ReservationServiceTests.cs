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
        public async Task CreateReservationAsync_ShouldThrowException_WhenEventNotFound()
        {
            // Arrange
            var reservation = new Reservation { Id = 1, EventId = 999, UserId = "user1" };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _reservationService.CreateReservationAsync(reservation));
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
        public async Task DeleteReservationAsync_ShouldReturnFalse_WhenReservationNotFound()
        {
            // Act
            var result = await _reservationService.DeleteReservationAsync(999);

            // Assert
            Assert.False(result);
        }

        
    }
}
