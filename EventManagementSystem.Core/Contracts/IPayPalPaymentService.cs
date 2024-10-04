using EventManagementSystem.Infrastructure.Data.Enums;
using EventManagementSystem.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PayPal.Api;


namespace EventManagementSystem.Core.Contracts
{
   
        public interface IPayPalPaymentService
        {
        Task<string> CreateOrderAsync(decimal amount, int reservationId);
        Task<bool> CaptureOrderAsync(string orderId);
        }
}

