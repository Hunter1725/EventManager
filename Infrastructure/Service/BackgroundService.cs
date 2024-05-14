using Contract.Services;
using Domain.Interfaces.Services;
using Hangfire;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public class BackgroundService : IBackgroundService
    {
        private readonly IEmailService _emailService;
        private readonly IEventService _eventService;
        private readonly ILogger<BackgroundService> _logger;

        public BackgroundService(
            IEmailService emailService,
            ILogger<BackgroundService> logger,
            IEventService eventService)
        {
            _emailService = emailService;
            _logger = logger;
            _eventService = eventService;
        }

        public async Task ProcessEventRemindersAsync()
        {
            var events = _eventService.GetEvents();
            foreach (var item in events)
            {
                if (item.Time.AddHours(-1) <= DateTime.Now)
                {
                    StringBuilder weatherList = new StringBuilder();
                    foreach (var weather in item.WeatherInfos)
                    {
                        weatherList.Append(weather.Time + ": " + weather.Description + "\n");
                    }
                    await SendEmailAsync(item.OwnerEmail, item.OwnerEmail,
                        $"Event {item.Title} will start in 1 hour. The weather in day: {weatherList}");
                }
            }
        }

        public async Task SendEmailAsync(string receiver, string subject, string body)
        {
            _logger.LogInformation($"Automatically sending email to {receiver}");
            await _emailService.SendEmailWith3rdEmailJs(receiver, subject, body);
        }

    }
}
