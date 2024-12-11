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
        private readonly DbContextOptions<EventDbContext> _options;

        public ReservationServiceTests()
        {
            _options = new DbContextOptionsBuilder<EventDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private ReservationService CreateService(EventDbContext dbContext)
        {
            return new ReservationService(dbContext);
        }

        private async Task SeedDataAsync(EventDbContext dbContext)
        {
            
            var users = new List<ApplicationUser>
    {
        new ApplicationUser { Id = "user1", UserName = "Test User" },
        new ApplicationUser { Id = "user2", UserName = "Another User" }
    };
            dbContext.Users.AddRange(users);

            
            dbContext.Venues.AddRange(
                new Venue { Id = 1, Name = "Main Hall", Address = "123 Main St", Capacity = 100 },
                new Venue { Id = 2, Name = "Conference Room", Address = "456 Side Ave", Capacity = 50 }
            );

            
            dbContext.Events.AddRange(
                new Event
                {
                    Id = 1,
                    Name = "Upcoming Event",
                    Date = DateTime.UtcNow.AddDays(5),
                    VenueId = 1,
                    TicketPrice = 50,
                    Description = "Event description",
                    OrganizerId = "user1" 
                },
                new Event
                {
                    Id = 2,
                    Name = "Past Event",
                    Date = DateTime.UtcNow.AddDays(-5),
                    VenueId = 2,
                    TicketPrice = 100,
                    Description = "Event description",
                    OrganizerId = "user1" 
                }
            );

            
            dbContext.Reservations.Add(new Reservation
            {
                Id = 1,
                EventId = 1,
                UserId = "user1",
                AttendeesCount = 2,
                IsPaid = true,
                PaymentDate = DateTime.UtcNow.AddDays(-1),
                TotalAmount = 100
            });

            await dbContext.SaveChangesAsync();
        }


        [Fact]
        public async Task CreateReservationAsync_AddsReservationSuccessfully()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);
            var service = CreateService(dbContext);

            var newReservation = new Reservation
            {
                EventId = 1,
                UserId = "user2",
                AttendeesCount = 3
            };

            var reservation = await service.CreateReservationAsync(newReservation);

            Assert.NotNull(reservation);
            Assert.Equal(150, reservation.TotalAmount); 
            Assert.Equal(97, dbContext.Venues.First(v => v.Id == 1).Capacity); 
        }

        [Fact]
        public async Task CreateReservationAsync_ThrowsIfEventNotFound()
        {
            using var dbContext = new EventDbContext(_options);
            var service = CreateService(dbContext);

            var newReservation = new Reservation
            {
                EventId = 99, 
                UserId = "user2",
                AttendeesCount = 3
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateReservationAsync(newReservation));
        }

        [Fact]
        public async Task CreateReservationAsync_ThrowsIfVenueCapacityExceeded()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);
            var service = CreateService(dbContext);

            var newReservation = new Reservation
            {
                EventId = 1,
                UserId = "user2",
                AttendeesCount = 200 
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateReservationAsync(newReservation));
        }

        [Fact]
        public async Task CreateReservationAsync_ThrowsIfReservationAlreadyExists()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);
            var service = CreateService(dbContext);

            var newReservation = new Reservation
            {
                EventId = 1,
                UserId = "user1", 
                AttendeesCount = 1
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateReservationAsync(newReservation));
        }

        [Fact]
        public async Task GetReservationByIdAsync_ReturnsReservation()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);
            var service = CreateService(dbContext);

            var reservation = await service.GetReservationByIdAsync(1);

            Assert.NotNull(reservation);
            Assert.Equal(1, reservation.Id);
            Assert.NotNull(reservation.Event);
            Assert.NotNull(reservation.Event.Venue);
        }

        [Fact]
        public async Task GetAllReservationsAsync_ReturnsAllReservations()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);
            var service = CreateService(dbContext);

            var reservations = await service.GetAllReservationsAsync();

            Assert.Single(reservations);
        }

        [Fact]
        public async Task UpdateReservationAsync_UpdatesReservationSuccessfully()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);
            var service = CreateService(dbContext);

            var updatedReservation = new Reservation
            {
                Id = 1,
                IsPaid = false,
                PaymentDate = DateTime.UtcNow.AddDays(2)
            };

            var reservation = await service.UpdateReservationAsync(updatedReservation);

            Assert.NotNull(reservation);
            Assert.False(reservation.IsPaid);
            Assert.Equal(DateTime.UtcNow.AddDays(2).Date, reservation.PaymentDate?.Date);
        }

        [Fact]
        public async Task UpdateReservationAsync_ThrowsIfReservationNotFound()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);
            var service = CreateService(dbContext);

            var updatedReservation = new Reservation
            {
                Id = 99, 
                IsPaid = true
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateReservationAsync(updatedReservation));
        }

        [Fact]
        public async Task DeleteReservationAsync_RemovesReservationSuccessfully()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);
            var service = CreateService(dbContext);

            var result = await service.DeleteReservationAsync(1);

            Assert.True(result);
            Assert.Null(await dbContext.Reservations.FindAsync(1));
            Assert.Equal(102, dbContext.Venues.First(v => v.Id == 1).Capacity); 
        }

        [Fact]
        public async Task DeleteReservationAsync_ReturnsFalseIfNotFound()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);
            var service = CreateService(dbContext);

            var result = await service.DeleteReservationAsync(99); 

            Assert.False(result);
        }

        [Fact]
        public async Task GetReservationsByEventIdAsync_ReturnsReservations()
        {
            using var dbContext = new EventDbContext(_options);
            await SeedDataAsync(dbContext);
            var service = CreateService(dbContext);

            var reservations = await service.GetReservationsByEventIdAsync(1);

            Assert.Single(reservations);
            Assert.Equal(1, reservations.First().EventId);
        }
    }
}
