using Contract.Repositories;
using Contract.Services;
using Domain.DTOs.EventDTO;
using Domain.DTOs.WeatherDTO;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces.Services;
using Domain.ResponseMessage;
using Infrastructure.Extensions;
using Infrastructure.Service;
using Infrastructure.UnitOfWork;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagerTest
{
    [TestFixture]
    public class EventServiceTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IWeatherService> _mockWeatherService;
        private Mock<IOpenWeatherService> _mockOpenWeatherService;
        private Mock<ILogger<EventService>> _mockLogger;
        private Mock<ICacheService> _mockCacheService;
        private IEventService _eventService;
        private readonly User user = new User { Id = 1, Username = "TestUser", Email = "test" };
        private readonly List<Weather> listWeather = new List<Weather>
        {
            new Weather
            {
                Location = "Test Location",
                Time = DateTime.Now,
                Temperature = 20,
                Humidity = 50,
                WindSpeed = 10,
                Description = "Cloudy",
                EventId = 1
            }
        };

        private readonly List<WeatherForCreate> weatherForCreate = new List<WeatherForCreate>
            {
                new WeatherForCreate
                {
                    Time = DateTime.Now,
                    Temperature = 20,
                    Description = "Cloudy"
                },

                new WeatherForCreate
                {
                    Time = DateTime.Now.AddDays(1),
                    Temperature = 25,
                    Description = "Sunny"
                }
            };


        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockWeatherService = new Mock<IWeatherService>();
            _mockOpenWeatherService = new Mock<IOpenWeatherService>();
            _mockLogger = new Mock<ILogger<EventService>>();
            _mockCacheService = new Mock<ICacheService>();

            _eventService = new EventService(
                _mockUnitOfWork.Object,
                _mockWeatherService.Object,
                _mockOpenWeatherService.Object,
                _mockLogger.Object,
                _mockCacheService.Object);
        }

        [Test]
        public async Task CreateEvent_ValidEventForCreate_ShouldCreateEvent()
        {
            // Arrange
            var eventForCreate = new EventForCreate
            {
                Title = "Test Event",
                Description = "Test Description",
                Location = "Test Location",
                Time = DateTime.Now
            };
            var id = 1; // Sample user ID
            _mockUnitOfWork.Setup(u => u.EventRepository.Add(It.IsAny<Event>()));
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);
            _mockOpenWeatherService.Setup(o => o.GetWeatherSpecificDate(It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(weatherForCreate);

            // Act
            await _eventService.CreateEvent(eventForCreate, id);

            // Assert
            _mockUnitOfWork.Verify(u => u.EventRepository.Add(It.IsAny<Event>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Exactly(2));
            _mockWeatherService.Verify(w => w.CreateWeather(It.IsAny<WeatherForCreate>()), Times.Exactly(2));
        }

        [Test]
        public void GetEvents_ShouldReturnEvents_WhenCacheIsEmpty()
        {
            // Arrange
            var events = new List<Event>
            {
                new Event {
                    Id = 1,
                    Title = "Event 1" ,
                    Description = "Event description 1",
                    Location = "Tst location",
                    Time = DateTime.Now,
                    UserId = 1,
                    User = user,
                    WeatherInfos = listWeather},

                new Event
                {
                    Id = 2,
                    Title = "Event 1" ,
                    Description = "Event description 1",
                    Location = "Tst location",
                    Time = DateTime.Now,
                    UserId = 1,
                    User = user,
                    WeatherInfos = listWeather
                }
            };

            var cachedEvents = events.Select(e => e.ResponseToRead()).ToList();

            _mockUnitOfWork.Setup(u => u.EventRepository.GetEvent()).Returns(events);
            _mockCacheService.Setup(mc => mc.TryGetValue(It.IsAny<string>(), out cachedEvents)).Returns(false);

            // Act
            var result = _eventService.GetEvents();

            // Assert
            Assert.That(result.Count(), Is.EqualTo(cachedEvents.Count()));
            _mockUnitOfWork.Verify(uow => uow.EventRepository.GetEvent(), Times.Once);
            _mockCacheService.Verify(mc => mc.Set(It.IsAny<string>(), It.IsAny<IEnumerable<EventForRead>>(), It.IsAny<MemoryCacheEntryOptions>()), Times.Once);
        }

        [Test]
        public void GetEventsByUserId_ShouldReturnEvents_WhenCacheEmpty()
        {
            // Arrange
            var events = new List<Event>
            {
                new Event {
                    Id = 1,
                    Title = "Event 1" ,
                    Description = "Event description 1",
                    Location = "Tst location",
                    Time = DateTime.Now,
                    UserId = 1,
                    User = user,
                    WeatherInfos = listWeather}
            };

            var cachedEvents = events.Select(e => e.ResponseToRead()).ToList();

            _mockUnitOfWork.Setup(u => u.EventRepository.GetEventByUserId(It.IsAny<int>())).Returns(events);
            _mockCacheService.Setup(mc => mc.TryGetValue(It.IsAny<string>(), out cachedEvents)).Returns(false);

            // Act
            var result = _eventService.GetEventsByUserId(It.IsAny<int>());

            // Assert
            Assert.That(result.Count, Is.EqualTo(cachedEvents.Count));
            _mockUnitOfWork.Verify(uow => uow.EventRepository.GetEventByUserId(It.IsAny<int>()), Times.Once);
            _mockCacheService.Verify(mc => mc.Set(It.IsAny<string>(), It.IsAny<IEnumerable<EventForRead>>(), It.IsAny<MemoryCacheEntryOptions>()), Times.Once);
        }

        [Test]
        public async Task GetEventById_ShouldReturnEvent_WhenEventExists()
        {
            // Arrange
            var eventId = 1;
            var eventEntity = new Event
            {
                Id = 1,
                Title = "Event 1",
                Description = "Event description 1",
                Location = "Tst location",
                Time = DateTime.Now,
                UserId = 1,
                User = user,
                WeatherInfos = listWeather
            };
            _mockUnitOfWork.Setup(uow => uow.EventRepository.GetEventById(eventId)).Returns(eventEntity);

            // Act
            var result = await _eventService.GetEventById(eventId);

            // Assert
            Assert.NotNull(result);
            Assert.That(result.Title, Is.EqualTo(eventEntity.Title));
        }

        [Test]
        public void GetEventById_ShouldThrowNotFoundException_WhenEventDoesNotExist()
        {
            // Arrange
            var eventId = 1;
            _mockUnitOfWork.Setup(uow => uow.EventRepository.GetEventById(eventId)).Returns((Event)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(() => _eventService.GetEventById(eventId));
            Assert.That(ex.Message, Is.EqualTo(ErrorMessage.EventNotFound));
        }

        [Test]
        public async Task UpdateEvent_ShouldUpdateEvent_WhenEventExists()
        {
            // Arrange
            var eventId = 1;
            var eventForUpdate = new EventForUpdate { 
                Title = "Updated Event", 
                Description = "Updated Description", 
                Location = "Updated Location", 
                Time = DateTime.Now 
            };
            var eventEntity =  new Event
            {
                Id = 1,
                Title = "Event 1",
                Description = "Event description 1",
                Location = "Tst location",
                Time = DateTime.Now,
                UserId = 1,
                User = user,
                WeatherInfos = listWeather
            };

            _mockUnitOfWork.Setup(uow => uow.EventRepository.GetById(eventId)).ReturnsAsync(eventEntity);
            _mockOpenWeatherService.Setup(ows => ows.GetWeatherSpecificDate(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(weatherForCreate);

            // Act
            await _eventService.UpdateEvent(eventId, eventForUpdate);

            // Assert
            Assert.That(eventEntity.Title, Is.EqualTo(eventForUpdate.Title));
            Assert.That(eventEntity.Description, Is.EqualTo(eventForUpdate.Description));
            Assert.That(eventEntity.Location, Is.EqualTo(eventForUpdate.Location));
            Assert.That(eventEntity.Time, Is.EqualTo(eventForUpdate.Time));

            _mockUnitOfWork.Verify(uow => uow.EventRepository.Update(eventEntity), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void UpdateEvent_ShouldThrowNotFoundException_WhenEventDoesNotExist()
        {
            // Arrange
            var eventId = 1;
            var eventForUpdate = new EventForUpdate { Title = "Updated Event" };
            _mockUnitOfWork.Setup(uow => uow.EventRepository.GetById(eventId)).ReturnsAsync((Event)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(() => _eventService.UpdateEvent(eventId, eventForUpdate));
            Assert.That(ex.Message, Is.EqualTo(ErrorMessage.EventNotFound));
        }

        // Unit tests for DeleteEvent
        [Test]
        public async Task DeleteEvent_ShouldDeleteEvent_WhenEventExists()
        {
            // Arrange
            var eventId = 1;
            var eventEntity = new Event { Id = eventId, Title = "Test Event" };
            _mockUnitOfWork.Setup(uow => uow.EventRepository.GetById(eventId)).ReturnsAsync(eventEntity);

            // Act
            await _eventService.DeleteEvent(eventId);

            // Assert
            _mockUnitOfWork.Verify(uow => uow.EventRepository.Delete(eventEntity), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void DeleteEvent_ShouldThrowNotFoundException_WhenEventDoesNotExist()
        {
            // Arrange
            var eventId = 1;
            _mockUnitOfWork.Setup(uow => uow.EventRepository.GetById(eventId)).ReturnsAsync((Event)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(() => _eventService.DeleteEvent(eventId));
            Assert.That(ex.Message, Is.EqualTo(ErrorMessage.EventNotFound));
        }
    }
}
