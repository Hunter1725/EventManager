using Domain.DTOs.EventDTO;
using Domain.DTOs.UserDTO;
using Domain.DTOs.WeatherDTO;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Extensions
{
    public static class Mapping
    {
        public static UserForRead ResponseToRead(this User user)
        {
            return new UserForRead
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }

        public static EventForRead ResponseToRead(this Event eventEntity)
        {
            return new EventForRead
            {
                Title = eventEntity.Title,
                Description = eventEntity.Description,
                Location = eventEntity.Location,
                Time = eventEntity.Time,
                UserId = eventEntity.UserId,
                OwnerEmail = eventEntity.User.Email,
                WeatherInfos = eventEntity.WeatherInfos.Select(x => x.ResponseToRead()).ToList()
            };
        }

        public static WeatherForRead ResponseToRead(this Weather weather)
        {
            return new WeatherForRead
            {
                Location = weather.Location,
                Time = weather.Time,
                Temperature = weather.Temperature,
                Humidity = weather.Humidity,
                WindSpeed = weather.WindSpeed,
                Description = weather.Description
            };
        }
    }
}
