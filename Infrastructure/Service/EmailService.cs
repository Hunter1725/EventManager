using Domain.Interfaces.Services;
using System.Net.Mail;
using System.Net;
using Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using Azure.Core;

namespace Infrastructure.Service
{
    public class EmailService : IEmailService
    {
        private readonly EmailConfig _config;
        private readonly ILogger<EmailService> logger;

        public EmailService(IOptions<EmailConfig> config, ILogger<EmailService> logger)
        {
            _config = config.Value;
            this.logger = logger;
        }

        public async Task SendEmailAsync(string receiver, string subject, string body)
        {
            string emailForSend = _config.EmailForSend;
            string appPasswordConfiguration = _config.AppPasswordConfiguration;

            SmtpClient smtpClient = new SmtpClient
            {
                Port = 587,
                EnableSsl = true,
                Host = "smtp.gmail.com",
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(emailForSend, appPasswordConfiguration),
            };

            MailMessage message = new MailMessage
            {
                Subject = subject,
                Body = body,
                From = new MailAddress(emailForSend),
            };

            message.To.Add(receiver);
            await smtpClient.SendMailAsync(message);

            //Log information of email sent
            logger.LogInformation($"Email sent to {receiver} with subject: {subject} and body {body}");
        }

        public async Task SendEmailWith3rdEmailJs(string receiver, string nameOfReceiver, string body)
        {
            // Construct the request data
            var requestData = new
            {
                service_id = _config.ServiceId,
                template_id = _config.TemplateId,
                user_id = _config.UserId,
                template_params = new
                {
                    message = body,
                    to_name = nameOfReceiver,
                    from_name = "Event Manager",
                    to_email = receiver,
                    reply_to = _config.EmailForSend
                },
                accessToken = _config.AccessToken
            };

            // Serialize the request data to JSON
            var requestDataJson = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);

            // Create HttpClient instance
            using (var httpClient = new HttpClient())
            {
                // Configure HttpClient
                httpClient.BaseAddress = new Uri(_config.Url);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                // Create StringContent with JSON data
                var content = new StringContent(requestDataJson, Encoding.UTF8, "application/json");

                try
                {
                    // Send POST request
                    HttpResponseMessage response = await httpClient.PostAsync("", content);

                    // Check if request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        logger.LogInformation("Your mail is sent!");
                    }
                    else
                    {
                        logger.LogError($"Oops... {await response.Content.ReadAsStringAsync()}");
                    }
                }
                catch (HttpRequestException ex)
                {
                    logger.LogError($"Oops... {ex.Message}");
                }
            }
        }
    }
}
