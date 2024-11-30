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
    public class VenueServiceTests
    {
        private readonly EventDbContext _context;
        private readonly VenueService _venueService;

        private void ClearDatabase()
        {
            _context.Venues.RemoveRange(_context.Venues);
            _context.SaveChanges();
        }
        public VenueServiceTests()
        {
            var options = new DbContextOptionsBuilder<EventDbContext>()
                .UseInMemoryDatabase(databaseName: "VenueServiceTests")
                .Options;

            _context = new EventDbContext(options);
            _venueService = new VenueService(_context);
        }

        [Fact]
        public async Task GetAllVenuesAsync_ShouldReturnAllVenues()
        {
            // Clear the database
            ClearDatabase();

            // Arrange
            var venues = new List<Venue>
    {
        new Venue { Id = 1, Name = "Venue 1", Address = "123 Main Street", Capacity = 100 },
        new Venue { Id = 2, Name = "Venue 2", Address = "456 Elm Street", Capacity = 200 }
    };

            _context.Venues.AddRange(venues);
            await _context.SaveChangesAsync();

            // Act
            var result = await _venueService.GetAllVenuesAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, v => v.Name == "Venue 1" && v.Address == "123 Main Street" && v.Capacity == 100);
            Assert.Contains(result, v => v.Name == "Venue 2" && v.Address == "456 Elm Street" && v.Capacity == 200);
        }

        [Fact]
        public async Task GetVenueByIdAsync_ShouldReturnVenue_WhenVenueExists()
        {
            // Arrange
            var venue = new Venue { Id = 1, Name = "Venue 1", Address = "123 Main Street", Capacity = 100 };
            _context.Venues.Add(venue);
            await _context.SaveChangesAsync();

            // Act
            var result = await _venueService.GetVenueByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Venue 1", result.Name);
            Assert.Equal("123 Main Street", result.Address);
            Assert.Equal(100, result.Capacity);
        }

        [Fact]
        public async Task GetVenueByIdAsync_ShouldReturnNull_WhenVenueDoesNotExist()
        {
            // Act
            var result = await _venueService.GetVenueByIdAsync(999);

            // Assert
            Assert.Null(result);
        }
    }
}
