using EventMaganementSystem.Data;
using EventManagementSystem.Core.Services;
using EventManagementSystem.Infrastructure.Data.Entities;
using EventManagementSystem.Infrastructure.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ServicesTests
{
    public class EventInvitationServiceTests
    {
        private readonly Mock<EventDbContext> _contextMock;
        private readonly EventInvitationService _invitationService;

        public EventInvitationServiceTests()
        {
            _contextMock = new Mock<EventDbContext>(new DbContextOptions<EventDbContext>());
            _invitationService = new EventInvitationService(_contextMock.Object);
        }

        
    }
}
