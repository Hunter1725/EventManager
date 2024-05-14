using Contract.Services;
using Domain.DTOs.WeatherDTO;
using Domain.Interfaces.Services;
using Domain.ResponseMessage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeathersController : ControllerBase
    {
        private readonly IWeatherService _weatherService;
        private readonly IOpenWeatherService _openWeatherService;

        public WeathersController(IWeatherService weatherService, IOpenWeatherService openWeatherService)
        {
            _weatherService = weatherService;
            _openWeatherService = openWeatherService;
        }

        //Get weather by event id
        [HttpGet("event/{eventId}")]
        [Authorize(Roles = "Admin,User")]
        public IActionResult GetWeather(int eventId)
        {
            try
            {
                var weather = _weatherService.GetWeathersByEventId(eventId);
                return Ok(weather);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Get weather by id
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetWeatherById(int id)
        {
            try
            {
                var weather = await _weatherService.GetWeatherById(id);
                return Ok(weather);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Create weather
        //[HttpPost]
        //[Authorize(Roles = "Admin,User")]
        //public async Task<IActionResult> CreateWeather([FromBody] WeatherForCreate weatherForCreate)
        //{
        //    try
        //    {
        //        await _weatherService.CreateWeather(weatherForCreate);
        //        return Ok("Create successfully!");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        //Update weather
        //[HttpPut("{id}")]
        //[Authorize(Roles = "Admin,User")]
        //public async Task<IActionResult> UpdateWeather(int id, [FromBody] WeatherForUpdate weatherForUpdate)
        //{
        //    try
        //    {
        //        await _weatherService.UpdateWeather(id, weatherForUpdate);
        //        return Ok("Update successfully!");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        //Delete weather
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> DeleteWeather(int id)
        {
            try
            {
                await _weatherService.DeleteWeather(id);
                return Ok(SuccessMessage.DeleteWeather);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Test get information from OpenWeatherAPI
        [HttpGet("test/{city}")]
        public async Task<IActionResult> Test(string city)
        {
            try
            {
                var weather = await _openWeatherService.GetWeatherDataTommorrow(city);
                return Ok(weather);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Test get information from OpenWeatherAPI
        [HttpGet("test2/{city}&{date}")]
        public async Task<IActionResult> Test2(string city, DateTime date)
        {
            try
            {
                var weather = await _openWeatherService.GetWeatherSpecificDate(city, date);
                return Ok(weather);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
