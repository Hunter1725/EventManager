using AutoMapper;
using Contract.Services;
using Domain.DTOs.WeatherDTO;
using Infrastructure.Extensions;
using Infrastructure.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public class WeatherService : IWeatherService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WeatherService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Get all weather by event id
        public IEnumerable<WeatherForRead> GetWeathersByEventId(int eventId)
        {
            var weathers = _unitOfWork.WeatherRepository
                .FindByCondition(w => w.EventId == eventId, false);
            return weathers.Select(x => x.ResponseToRead());
        }

        public async Task<WeatherForRead> GetWeatherById(int id)
        {
            var weatherEntity = await _unitOfWork.WeatherRepository.GetById(id);
            if (weatherEntity == null)
            {
                throw new Exception("Weather not found!");
            }
            return weatherEntity.ResponseToRead();
        }

        public async Task CreateWeather(WeatherForCreate weatherForCreate)
        {
            var weatherEntity = new Domain.Entities.Weather
            {
                Location = weatherForCreate.Location,
                Time = weatherForCreate.Time,
                Temperature = weatherForCreate.Temperature,
                Humidity = weatherForCreate.Humidity,
                WindSpeed = weatherForCreate.WindSpeed,
                Description = weatherForCreate.Description,
                EventId = weatherForCreate.EventId
            };

            _unitOfWork.WeatherRepository.Add(weatherEntity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateWeather(int id, WeatherForUpdate weatherForCreate)
        {
            var weatherEntity = await _unitOfWork.WeatherRepository.GetById(id);
            if (weatherEntity == null)
            {
                throw new Exception("Weather not found!");
            }

            weatherEntity.Location = weatherForCreate.Location;
            weatherEntity.Time = weatherForCreate.Time;
            weatherEntity.Temperature = weatherForCreate.Temperature;
            weatherEntity.Humidity = weatherForCreate.Humidity;
            weatherEntity.WindSpeed = weatherForCreate.WindSpeed;
            weatherEntity.Description = weatherForCreate.Description;

            _unitOfWork.WeatherRepository.Update(weatherEntity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteWeather(int id)
        {
            var weatherEntity = await _unitOfWork.WeatherRepository.GetById(id);
            if (weatherEntity == null)
            {
                throw new Exception("Weather not found!");
            }

            _unitOfWork.WeatherRepository.Delete(weatherEntity);
            await _unitOfWork.SaveChangesAsync();
        }

        //Delete all weather by event id
        public async Task DeleteWeathersByEventId(int eventId)
        {
            var weathers = _unitOfWork.WeatherRepository
                .FindByCondition(w => w.EventId == eventId, true);
            if(weathers.Count() == 0)
            {
                throw new Exception("Weather not found!");
            }

            foreach (var weather in weathers)
            {
                _unitOfWork.WeatherRepository.Delete(weather);
            }
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
