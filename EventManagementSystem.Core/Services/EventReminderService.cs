using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Core.Services
{
    public class EventReminderService : BackgroundService
    {
        private readonly INotificationService _notificationService;
        private readonly IServiceProvider _serviceProvider;

        public EventReminderService(INotificationService notificationService, IServiceProvider serviceProvider)
        {
            _notificationService = notificationService;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<EventDbContext>();
                    var events = await dbContext.Events
                        .Where(e => e.Date.Date == DateTime.UtcNow.Date.AddDays(1))
                        .ToListAsync(stoppingToken);

                    foreach (var eventItem in events)
                    {
                        var attendees = await dbContext.Reservations
                            .Where(r => r.EventId == eventItem.Id)
                            .Select(r => r.UserId)
                            .ToListAsync(stoppingToken);

                        foreach (var userId in attendees)
                        {
                            await _notificationService.NotifyEventReminderAsync(userId, eventItem.Name, eventItem.Date);
                        }
                    }
                }

                
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }

}

