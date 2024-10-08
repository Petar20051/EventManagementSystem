﻿using EventManagementSystem.Infrastructure.Data.Enums;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Contracts
{
    public interface IStripePaymentService
    {
        Task<string> CreateCheckoutSession(decimal amount, int reservationId);
    }
}
