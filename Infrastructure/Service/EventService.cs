using AutoMapper;
using Contract.Services;
using Domain.Entities;
using Domain.DTOs.EventDTO;
using Infrastructure.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Domain.Exceptions;
using Domain.ResponseMessage;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Service
{
    public class EventService : IEventService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWeatherService _weatherService;
        private readonly IOpenWeatherService _openWeatherService;
        private readonly ILogger<EventService> _logger;
        private readonly ICacheService _cacheService;
        private readonly string cacheKey = "Events";
        private readonly string cacheKey2 = "EventsByUser";

        public EventService(
            IUnitOfWork unitOfWork,
            IWeatherService weatherService,
            IOpenWeatherService openWeatherService,
            ILogger<EventService> logger,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _weatherService = weatherService;
            _openWeatherService = openWeatherService;
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task CreateEvent(EventForCreate eventForCreate, int id)
        {
            var eventEntity = new Event
            {
                Title = eventForCreate.Title,
                Description = eventForCreate.Description,
                Location = eventForCreate.Location,
                Time = eventForCreate.Time,
                UserId = id
            };

            _unitOfWork.EventRepository.Add(eventEntity);
            await _unitOfWork.SaveChangesAsync();

            //Create weather info for event
            var weatherForecast = await _openWeatherService
                .GetWeatherSpecificDate(eventForCreate.Location.Replace(" ", "").ToLower(), eventForCreate.Time);

            foreach (var weather in weatherForecast)
            {
                weather.EventId = eventEntity.Id;
                await _weatherService.CreateWeather(weather);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        public IEnumerable<EventForRead> GetEvents()
        {
            if (!_cacheService.TryGetValue(cacheKey, out IEnumerable<EventForRead> events))
            {
                events = _unitOfWork.EventRepository.GetEvent().Select(e => e.ResponseToRead());

                // Cache the fetched data with a sliding expiration of 10 seconds
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(10));
                _cacheService.Set(cacheKey, events, cacheEntryOptions);
            }
            return events;
        }

        // Get all events of current user
        public IEnumerable<EventForRead> GetEventsByUserId(int userId)
        {
            if (!_cacheService.TryGetValue(cacheKey2, out IEnumerable<EventForRead> events))
            {
                events = _unitOfWork.EventRepository.GetEventByUserId(userId).Select(e => e.ResponseToRead());

                // Cache the fetched data with a sliding expiration of 10 seconds
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(10));
                _cacheService.Set(cacheKey2, events, cacheEntryOptions);
            }
            return events;
        }

        public async Task<EventForRead> GetEventById(int id)
        {
            var eventEntity = _unitOfWork.EventRepository
                .GetEventById(id);

            if (eventEntity == null)
            {
                //Log error
                _logger.LogError("Event with id {0} not found to get", id);
                throw new NotFoundException(ErrorMessage.EventNotFound);
            }
            return eventEntity.ResponseToRead();
        }

        public async Task UpdateEvent(int id, EventForUpdate eventForUpdate)
        {
            var eventEntity = await _unitOfWork.EventRepository.GetById(id);
            if (eventEntity == null)
            {
                _logger.LogError("Event with id {0} not found to update", id);
                throw new NotFoundException(ErrorMessage.EventNotFound);
            }
            eventEntity.Title = eventForUpdate.Title;
            eventEntity.Description = eventForUpdate.Description;
            eventEntity.Location = eventForUpdate.Location;
            eventEntity.Time = eventForUpdate.Time;

            _unitOfWork.EventRepository.Update(eventEntity);

            //Update weather: delete curent weather and create new weather
            await _weatherService.DeleteWeathersByEventId(id);

            var weatherForecast = await _openWeatherService
                .GetWeatherSpecificDate(eventForUpdate.Location.Replace(" ", "").ToLower(), eventForUpdate.Time);

            foreach (var weather in weatherForecast)
            {
                weather.EventId = id;
                await _weatherService.CreateWeather(weather);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteEvent(int id)
        {
            var eventEntity = await _unitOfWork.EventRepository.GetById(id);
            if (eventEntity == null)
            {
                _logger.LogError("Event with id {0} not found to delete", id);
                throw new NotFoundException(ErrorMessage.EventNotFound);
            }
            _unitOfWork.EventRepository.Delete(eventEntity);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
