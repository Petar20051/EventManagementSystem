﻿using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Services
{
    public class UserEventService:IUserEventService
    {
        private readonly EventDbContext _context;

        public UserEventService(EventDbContext context)
        {
            _context = context;
        }

        public async Task AddUserEventAsync(string userId, int eventId)
        {
            var userEvent = new UserEvent
            {
                UserId = userId,
                EventId = eventId
            };
            _context.UserEvents.Add(userEvent);
            await _context.SaveChangesAsync();
        }
    }
}
