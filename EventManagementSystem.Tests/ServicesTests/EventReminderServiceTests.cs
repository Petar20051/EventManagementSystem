using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Services;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ServicesTests
{
    public class EventReminderServiceTests
    {
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly EventDbContext _dbContext;
        private readonly EventReminderService _eventReminderService;

        public EventReminderServiceTests()
        {
            // In-Memory Database setup
            var options = new DbContextOptionsBuilder<EventDbContext>()
                .UseInMemoryDatabase(databaseName: "EventDb")
                .Options;

            _dbContext = new EventDbContext(options);

            // Mock Notification Service
            _notificationServiceMock = new Mock<INotificationService>();

            // Mock IServiceScopeFactory and IServiceScope
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceScopeMock = new Mock<IServiceScope>();

            // Mock IServiceProvider to return the EventDbContext and other services
            _serviceProviderMock = new Mock<IServiceProvider>();

            _serviceScopeFactoryMock
                .Setup(f => f.CreateScope())
                .Returns(_serviceScopeMock.Object);

            _serviceScopeMock
                .Setup(s => s.ServiceProvider)
                .Returns(_serviceProviderMock.Object);

            _serviceProviderMock
                .Setup(sp => sp.GetService(typeof(EventDbContext)))
                .Returns(_dbContext);

            // Initialize the EventReminderService
            _eventReminderService = new EventReminderService(_notificationServiceMock.Object, _serviceProviderMock.Object);
        }

        // Cleanup the in-memory database before each test
        public void ClearDatabase()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Database.EnsureCreated();
        }

        
    }
}
