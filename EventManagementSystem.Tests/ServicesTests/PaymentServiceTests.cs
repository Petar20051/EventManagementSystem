using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Extensions;
using EventManagementSystem.Core.Models.Payments;
using EventManagementSystem.Core.Services;
using EventManagementSystem.Infrastructure.Data.Entities;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;

using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ServicesTests
{
    public class PaymentServiceTests
    {
        private readonly Mock<EventDbContext> _contextMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IPaymentMethodServiceWrapper> _paymentMethodServiceWrapperMock;
        private readonly PaymentService _paymentService;

        public PaymentServiceTests()
        {
            _contextMock = new Mock<EventDbContext>(new DbContextOptions<EventDbContext>());
            _userServiceMock = new Mock<IUserService>();
            _paymentMethodServiceWrapperMock = new Mock<IPaymentMethodServiceWrapper>();

            _paymentService = new PaymentService(
                _contextMock.Object,
                _userServiceMock.Object,
                _paymentMethodServiceWrapperMock.Object
            );
        }

        private Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
        {
            var mockDbSet = new Mock<DbSet<T>>();
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockDbSet;
        }

        
    }
}
