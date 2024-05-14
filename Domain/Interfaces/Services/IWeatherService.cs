using Domain.DTOs.WeatherDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Services
{
    public interface IWeatherService
    {
        IEnumerable<WeatherForRead> GetWeathersByEventId(int eventId);
        Task<WeatherForRead> GetWeatherById(int id);
        Task CreateWeather(WeatherForCreate weatherForCreate);
        Task UpdateWeather(int id, WeatherForUpdate weatherForCreate);
        Task DeleteWeather(int id);
        Task DeleteWeathersByEventId(int eventId);
    }
}
