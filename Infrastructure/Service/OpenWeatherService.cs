using Domain.DTOs.WeatherDTO;
using Domain.Interfaces.Services;
using Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public class OpenWeatherService : IOpenWeatherService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OpenWeatherAPIConfig _config;
        private readonly ILogger<OpenWeatherService> _logger;

        public OpenWeatherService(
            IHttpClientFactory httpClientFactory, 
            ILogger<OpenWeatherService> logger, 
            IOptions<OpenWeatherAPIConfig> config)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _config = config.Value;  
        }

        public async Task<IEnumerable<WeatherForCreate>> GetWeatherDataTommorrow(string city)
        {
            try
            {
                var apiUrl = _config.Url.Replace("{city}", city).Replace("{API_KEY}", _config.APIKey);

                var response = await _httpClientFactory.CreateClient().GetAsync(apiUrl);
                response.EnsureSuccessStatusCode(); // Ensure success status code

                var responseBody = await response.Content.ReadAsStringAsync();

                // Parse the JSON response
                dynamic forecast = JsonConvert.DeserializeObject(responseBody);

                //Log url
                _logger.LogInformation("Calling OpenWeatherMap API. URL: {0}", apiUrl);

                return ConvertFromJson(forecast);
            }
            catch (HttpRequestException ex)
            {
                //Log error message
                _logger.LogError(ex, "Error calling OpenWeatherMap API. URL: {0}", ex);
                throw new Exception("Error calling OpenWeatherMap API.", ex);
            }
        }

        private IEnumerable<WeatherForCreate> ConvertFromJson(dynamic jsonData)
        {
            var weatherList = jsonData.list;
            var city = jsonData.city;

            var weatherForecasts = new List<WeatherForCreate>();

            // Filter weather data for tomorrow
            var tomorrow = DateTime.Today.AddDays(1).Date;
            foreach (var weatherData in weatherList)
            {
                var time = DateTimeOffset.FromUnixTimeSeconds((long)weatherData.dt).DateTime;

                // Check if the forecast is for tomorrow
                if (time.Date == tomorrow)
                {
                    // Extracting data from the JSON response
                    var location = city.name;
                    var temperature = (decimal)weatherData.main.temp;
                    var humidity = (decimal)weatherData.main.humidity;
                    var windSpeed = (decimal)weatherData.wind.speed;
                    var description = weatherData.weather[0].description;

                    // Creating WeatherForCreate object for each forecast
                    var weatherForCreate = new WeatherForCreate
                    {
                        Location = location,
                        Time = time,
                        Temperature = temperature,
                        Humidity = humidity,
                        WindSpeed = windSpeed,
                        Description = description
                    };

                    weatherForecasts.Add(weatherForCreate);
                }
            }

            return weatherForecasts;
        }

        public async Task<IEnumerable<WeatherForCreate>> GetWeatherSpecificDate(string city, DateTime date)
        {
            try
            {
                var apiUrl = _config.Url.Replace("{city}", city).Replace("{API_KEY}", _config.APIKey);

                var response = await _httpClientFactory.CreateClient().GetAsync(apiUrl);
                response.EnsureSuccessStatusCode(); // Ensure success status code

                var responseBody = await response.Content.ReadAsStringAsync();

                // Parse the JSON response
                dynamic forecast = JsonConvert.DeserializeObject(responseBody);

                return ConvertFromJson(forecast, date);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling OpenWeatherMap API. URL: {0}", ex);
                throw new Exception("Error calling OpenWeatherMap API.", ex);
            }
        }

        private IEnumerable<WeatherForCreate> ConvertFromJson(dynamic jsonData, DateTime date)
        {
            var weatherList = jsonData.list;
            var city = jsonData.city;

            var weatherForecasts = new List<WeatherForCreate>();

            // Filter weather data for the specific date
            foreach (var weatherData in weatherList)
            {
                var time = DateTimeOffset.FromUnixTimeSeconds((long)weatherData.dt).DateTime;

                // Check if the forecast is for the specified date
                if (time.Date == date.Date)
                {
                    // Extracting data from the JSON response
                    var location = city.name;
                    var temperature = (decimal)weatherData.main.temp;
                    var humidity = (decimal)weatherData.main.humidity;
                    var windSpeed = (decimal)weatherData.wind.speed;
                    var description = weatherData.weather[0].description;

                    // Creating WeatherForCreate object for each forecast
                    var weatherForCreate = new WeatherForCreate
                    {
                        Location = location,
                        Time = time,
                        Temperature = temperature,
                        Humidity = humidity,
                        WindSpeed = windSpeed,
                        Description = description
                    };

                    weatherForecasts.Add(weatherForCreate);
                }
            }

            return weatherForecasts;
        }

    }
}
