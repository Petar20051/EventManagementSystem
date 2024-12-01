using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Services;
using EventManagementSystem.Infrastructure.Data.Enums;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ServicesTests
{
    public class NotificationServiceTests
    {
        private readonly Mock<EventDbContext> _contextMock;
        private readonly Mock<INotificationHub> _notificationHubMock;
        private readonly NotificationService _notificationService;

        public NotificationServiceTests()
        {
            _contextMock = new Mock<EventDbContext>(new DbContextOptions<EventDbContext>());
            _notificationHubMock = new Mock<INotificationHub>();

            _notificationService = new NotificationService(
                _contextMock.Object,
                _notificationHubMock.Object
            );
        }

       
    }
}
