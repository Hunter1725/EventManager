using Infrastructure.Configuration;
using Infrastructure.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagerTest
{
    [TestFixture]
    public class OpenWeatherServiceTests
    {
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private Mock<ILogger<OpenWeatherService>> _mockLogger;
        private Mock<IOptions<OpenWeatherAPIConfig>> _mockConfig;
        private OpenWeatherService _openWeatherService;

        [SetUp]
        public void Setup()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(_mockHttpMessageHandler.Object);

            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            _mockLogger = new Mock<ILogger<OpenWeatherService>>();
            _mockConfig = new Mock<IOptions<OpenWeatherAPIConfig>>();

            _mockConfig.Setup(config => config.Value).Returns(new OpenWeatherAPIConfig
            {
                Url = "http://api.openweathermap.org/data/2.5/forecast?q={city}&appid={API_KEY}",
                APIKey = "test_api_key"
            });

            _openWeatherService = new OpenWeatherService(_mockHttpClientFactory.Object, _mockLogger.Object, _mockConfig.Object);
        }

        private void SetupHttpResponse(string responseContent, HttpStatusCode statusCode)
        {
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(responseContent)
                });
        }

        private string GenerateMockWeatherResponse()
        {
            var forecast = new
            {
                list = new[]
                {
                    new
                    {
                        dt = new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds(),
                        main = new { temp = 300.15m, humidity = 80 },
                        wind = new { speed = 5.1m },
                        weather = new[] { new { description = "clear sky" } }
                    }
                },
                city = new { name = "Test City" }
            };

            return JsonConvert.SerializeObject(forecast);
        }

        [Test]
        public async Task GetWeatherDataTomorrow_ShouldReturnWeatherData()
        {
            // Arrange
            var city = "testcity";
            var responseContent = GenerateMockWeatherResponse();
            SetupHttpResponse(responseContent, HttpStatusCode.OK);

            // Act
            var result = await _openWeatherService.GetWeatherDataTommorrow(city);

            // Assert
            Assert.NotNull(result);
            Assert.That(result, Has.Exactly(1).Items);

            var weather = result.First();
            Assert.That(weather.Location, Is.EqualTo("Test City"));
            Assert.That(weather.Description, Is.EqualTo("clear sky"));
        }

        [Test]
        public void GetWeatherDataTomorrow_ShouldThrowException_OnHttpRequestException()
        {
            // Arrange
            var city = "testcity";
            SetupHttpResponse("", HttpStatusCode.BadRequest);

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _openWeatherService.GetWeatherDataTommorrow(city));
            Assert.That(ex.Message, Is.EqualTo("Error calling OpenWeatherMap API."));
        }

        [Test]
        public async Task GetWeatherSpecificDate_ShouldReturnWeatherData()
        {
            // Arrange
            var city = "testcity";
            var date = DateTime.Today.AddDays(1);
            var responseContent = GenerateMockWeatherResponse();
            SetupHttpResponse(responseContent, HttpStatusCode.OK);

            // Act
            var result = await _openWeatherService.GetWeatherSpecificDate(city, date);

            // Assert
            Assert.NotNull(result);
            Assert.That(result, Has.Exactly(1).Items);

            var weather = result.First();
            Assert.That(weather.Location, Is.EqualTo("Test City"));
            Assert.That(weather.Description, Is.EqualTo("clear sky"));
        }

        [Test]
        public void GetWeatherSpecificDate_ShouldThrowException_OnHttpRequestException()
        {
            // Arrange
            var city = "testcity";
            var date = DateTime.Today.AddDays(1);
            SetupHttpResponse("", HttpStatusCode.BadRequest);

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _openWeatherService.GetWeatherSpecificDate(city, date));
            Assert.That(ex.Message, Is.EqualTo("Error calling OpenWeatherMap API."));
        }
    }
}
