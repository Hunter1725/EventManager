using Domain.Interfaces.Services;
using Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;
using Infrastructure.Service;
using Moq.Protected;

namespace EventManagerTest
{
    [TestFixture]
    public class EmailServiceTest
    {
        private Mock<IOptions<EmailConfig>> _mockConfig;
        private Mock<ILogger<EmailService>> _mockLogger;
        private EmailService _emailService;
        private EmailConfig _emailConfig;

        [SetUp]
        public void Setup()
        {
            _emailConfig = new EmailConfig
            {
                EmailForSend = "test@example.com",
                AppPasswordConfiguration = "password",
                ServiceId = "service_id",
                TemplateId = "template_id",
                UserId = "user_id",
                AccessToken = "access_token",
                Url = "https://api.emailjs.com/api/v1.0/email/send"
            };

            _mockConfig = new Mock<IOptions<EmailConfig>>();
            _mockLogger = new Mock<ILogger<EmailService>>();

            _mockConfig.Setup(config => config.Value).Returns(_emailConfig);

            _emailService = new EmailService(_mockConfig.Object, _mockLogger.Object);
        }

        //[Test]
        //public async Task SendEmailAsync_ShouldSendEmail()
        //{
        //    // Arrange
        //    string receiver = "testReceiver@gmail.com";
        //    string subject = "testSubject";
        //    string body = "testBody";

        //    var mockSmtpClient = new Mock<SmtpClient>();
        //    mockSmtpClient.Setup(x => x.SendMailAsync(It.IsAny<MailMessage>())).Returns(Task.CompletedTask);

        //    // Act
        //    await _emailService.SendEmailAsync(receiver, subject, body);

        //    // Assert
        //    _mockLogger.Verify(
        //        x => x.Log(
        //        LogLevel.Information,
        //        It.IsAny<EventId>(),
        //        It.Is<It.IsAnyType>((v, t) => v.ToString()
        //        .Contains($"Email sent to {receiver} with subject: {subject} and body {body}")),
        //        null,
        //        It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        //}

        //[Test]
        //public async Task SendEmailWith3rdEmailJs_ShouldLogSuccessMessage_WhenRequestIsSuccessful()
        //{
        //    // Arrange
        //    var receiver = "receiver@example.com";
        //    var nameOfReceiver = "Receiver";
        //    var body = "Test Body";

        //    var handlerMock = new Mock<HttpMessageHandler>();
        //    handlerMock
        //        .Protected()
        //        .Setup<Task<HttpResponseMessage>>(
        //            "SendAsync",
        //            ItExpr.IsAny<HttpRequestMessage>(),
        //            ItExpr.IsAny<CancellationToken>())
        //        .ReturnsAsync(new HttpResponseMessage
        //        {
        //            StatusCode = HttpStatusCode.OK,
        //            Content = new StringContent("Success")
        //        });

        //    var httpClient = new HttpClient(handlerMock.Object)
        //    {
        //        BaseAddress = new Uri(_emailConfig.Url)
        //    };

        //    handlerMock.Setup(x => x.)

        //    // Act
        //    await _emailService.SendEmailWith3rdEmailJs(receiver, nameOfReceiver, body);

        //    // Assert
        //    _mockLogger.Verify(
        //        x => x.Log(
        //            LogLevel.Information,
        //            It.IsAny<EventId>(),
        //            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Your mail is sent!")),
        //            null,
        //            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        //        Times.Once);
        //}


        [Test]
        public async Task SendEmailWith3rdEmailJs_ShouldLogErrorMessage_WhenRequestFails()
        {
            // Arrange
            var receiver = "receiver@example.com";
            var nameOfReceiver = "Receiver";
            var body = "Test Body";

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Error")
                });

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri(_emailConfig.Url)
            };

            var service = new EmailService(_mockConfig.Object, _mockLogger.Object);

            // Act
            await service.SendEmailWith3rdEmailJs(receiver, nameOfReceiver, body);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Oops...")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
