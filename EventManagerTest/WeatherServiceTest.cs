using Contract.Services;
using Domain.DTOs.WeatherDTO;
using Domain.Entities;
using Infrastructure.Extensions;
using Infrastructure.Service;
using Infrastructure.UnitOfWork;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EventManagerTest
{
    [TestFixture]
    public class WeatherServiceTest
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private IWeatherService _weatherService;

        private readonly List<Weather> weathers = new List<Weather>
            {
                new Weather
                {
                    Id = 1,
                    Location = "Hanoi",
                    Time = DateTime.Now,
                    Temperature = 30,
                    Humidity = 80,
                    WindSpeed = 5,
                    Description = "Sunny"
                },
                new Weather
                {
                    Id = 2,
                    Location = "Hanoi",
                    Time = DateTime.Now,
                    Temperature = 30,
                    Humidity = 80,
                    WindSpeed = 5,
                    Description = "Sunny"
                }
            };

        private Weather weather = new Weather
        {
            Id = 1,
            Location = "Hanoi",
            Time = DateTime.Now,
            Temperature = 30,
            Humidity = 80,
            WindSpeed = 5,
            Description = "Sunny"
        };

        private Event newEvent = new Event
        {
            Id = 1,
            Title = "Event 1",
            Description = "Description 1",
            Location = "Hanoi",
            Time = DateTime.Now,
            UserId = 1
        };

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _weatherService = new WeatherService(_mockUnitOfWork.Object);
        }

        [Test]
        public void GetWeathersByEventId_WhenCalled_ReturnsWeathers()
        {
            // Arrange
            _mockUnitOfWork.Setup(m => m.WeatherRepository.FindByCondition(It.IsAny<Expression<Func<Weather, bool>>>(), false))
                .Returns(weathers);

            // Act
            var result = _weatherService.GetWeathersByEventId(1);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(weathers.Count));
        }

        [Test]
        public async Task GetWeatherById_WhenCalled_ReturnsWeather()
        {
            // Arrange
            _mockUnitOfWork.Setup(m => m.WeatherRepository.GetById(It.IsAny<int>()))
                .ReturnsAsync(weather);

            // Act
            var result = await _weatherService.GetWeatherById(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Humidity, Is.EqualTo(weather.ResponseToRead().Humidity));
            Assert.That(result.Location, Is.EqualTo(weather.ResponseToRead().Location));
            Assert.That(result.Temperature, Is.EqualTo(weather.ResponseToRead().Temperature));
        }

        [Test]
        public async Task GetWeatherById_InvalidId_ThrowException()
        {
            // Arrange
            _mockUnitOfWork.Setup(m => m.WeatherRepository.GetById(It.IsAny<int>()))
                .ReturnsAsync((Weather)null);

            // Act
            var ex = Assert.ThrowsAsync<Exception>(() => _weatherService.GetWeatherById(1));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Weather not found!"));
        }

        [Test]
        public async Task CreateWeather_WhenCalled_Success()
        {
            // Arrange
            var weatherForCreate = new WeatherForCreate
            {
                Location = "Hanoi",
                Time = DateTime.Now,
                Temperature = 30,
                Humidity = 80,
                WindSpeed = 5,
                Description = "Sunny",
                EventId = 1
            };
            _mockUnitOfWork.Setup(m => m.WeatherRepository.Add(It.IsAny<Weather>()));

            // Act
            await _weatherService.CreateWeather(weatherForCreate);

            // Assert
            _mockUnitOfWork.Verify(m => m.WeatherRepository.Add(It.IsAny<Weather>()), Times.Once);
            _mockUnitOfWork.Verify(m => m.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task UpdateWeather_WhenCalled_Success()
        {
            // Arrange
            var weatherForUpdate = new WeatherForUpdate
            {
                Location = "Hanoi",
                Time = DateTime.Now,
                Temperature = 30,
                Humidity = 80,
                WindSpeed = 5,
                Description = "Sunny"
            };
            _mockUnitOfWork.Setup(m => m.WeatherRepository.GetById(It.IsAny<int>()))
                .ReturnsAsync(weather);
            _mockUnitOfWork.Setup(m => m.WeatherRepository.Update(It.IsAny<Weather>()));

            // Act
            await _weatherService.UpdateWeather(1, weatherForUpdate);

            // Assert
            _mockUnitOfWork.Verify(m => m.WeatherRepository.Update(It.IsAny<Weather>()), Times.Once);
            _mockUnitOfWork.Verify(m => m.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task UpdateWeather_InvalidId_ThrowException()
        {
            // Arrange
            var weatherForUpdate = new WeatherForUpdate
            {
                Location = "Hanoi",
                Time = DateTime.Now,
                Temperature = 30,
                Humidity = 80,
                WindSpeed = 5,
                Description = "Sunny"
            };
            _mockUnitOfWork.Setup(m => m.WeatherRepository.GetById(It.IsAny<int>()))
                .ReturnsAsync((Weather)null);

            // Act
            var ex = Assert.ThrowsAsync<Exception>(() => _weatherService.UpdateWeather(1, weatherForUpdate));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Weather not found!"));
        }

        [Test]
        public async Task DeleteWeather_WhenCalled_Success()
        {
            // Arrange
            _mockUnitOfWork.Setup(m => m.WeatherRepository.GetById(It.IsAny<int>()))
                .ReturnsAsync(weather);
            _mockUnitOfWork.Setup(m => m.WeatherRepository.Delete(It.IsAny<Weather>()));

            // Act
            await _weatherService.DeleteWeather(1);

            // Assert
            _mockUnitOfWork.Verify(m => m.WeatherRepository.Delete(It.IsAny<Weather>()), Times.Once);
            _mockUnitOfWork.Verify(m => m.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task DeleteWeather_InvalidId_ThrowException()
        {
            // Arrange
            _mockUnitOfWork.Setup(m => m.WeatherRepository.GetById(It.IsAny<int>()))
                .ReturnsAsync((Weather)null);

            // Act
            var ex = Assert.ThrowsAsync<Exception>(() => _weatherService.DeleteWeather(1));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Weather not found!"));
        }

        [Test]
        public async Task DeleteWeathersByEventId_WhenCalled_Success()
        {
            // Arrange
            _mockUnitOfWork.Setup(m => m.WeatherRepository.FindByCondition(It.IsAny<Expression<Func<Weather, bool>>>(), It.IsAny<bool>()))
                .Returns(weathers);
            _mockUnitOfWork.Setup(m => m.WeatherRepository.Delete(It.IsAny<Weather>()));

            // Act
            await _weatherService.DeleteWeathersByEventId(1);

            // Assert
            _mockUnitOfWork.Verify(m => m.WeatherRepository.Delete(It.IsAny<Weather>()), Times.Exactly(weathers.Count));
            _mockUnitOfWork.Verify(m => m.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task DeleteWeathersByEventId_NoWeather_ThrowException()
        {
            // Arrange
            _mockUnitOfWork.Setup(m => m.WeatherRepository.FindByCondition(It.IsAny<Expression<Func<Weather, bool>>>(), It.IsAny<bool>()))
                .Returns(new List<Weather>());

            // Act
            var ex = Assert.ThrowsAsync<Exception>(() => _weatherService.DeleteWeathersByEventId(1));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Weather not found!"));
        }
    }
}
