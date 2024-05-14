using Domain.DTOs.WeatherDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Services
{
    public interface IOpenWeatherService
    {
        Task<IEnumerable<WeatherForCreate>> GetWeatherDataTommorrow(string city);
        Task<IEnumerable<WeatherForCreate>> GetWeatherSpecificDate(string city, DateTime date);
    }
}
