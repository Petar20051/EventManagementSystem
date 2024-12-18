﻿using EventManagementSystem.Core.Models.Payments;
using EventManagementSystem.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Contracts
{
    public interface IPaymentService
    {
        Task RecordPaymentAsync(Payment payment);

        Task DeleteCreditCardAsync(string cardId, string userId);
        Task<PaymentDetailsViewModel> GetPaymentByIdAsync(DateTime? reservationDate);


    }
}
