using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Infrastructure.Configuration;

public class OpenWeatherAPIHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenWeatherAPIHealthCheck> _logger;
    private readonly HttpClient _httpClient;

    public OpenWeatherAPIHealthCheck(
        IConfiguration configuration,
        ILogger<OpenWeatherAPIHealthCheck> logger,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        try
        {
            // Get the OpenWeatherAPI configuration from app settings
            var openWeatherConfig = _configuration.GetSection("OpenWeatherAPIConfig").Get<OpenWeatherAPIConfig>();

            // Create the URL for the OpenWeatherAPI
            var apiUrl = openWeatherConfig.Url
                .Replace("{city}", "hanoi")
                .Replace("{API_KEY}", openWeatherConfig.APIKey);

            // Send a request to the OpenWeatherAPI
            var response = await _httpClient.GetAsync(apiUrl, cancellationToken);

            // Check if the response is successful
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("OpenWeatherAPI is reachable");
            }
            else
            {
                // Log the error if the API request fails
                _logger.LogError($"OpenWeatherAPI request failed with status code: {response.StatusCode}");
                return HealthCheckResult.Unhealthy("OpenWeatherAPI is unreachable");
            }
        }
        catch (Exception ex)
        {
            // Log any exceptions that occur during the health check
            _logger.LogError(ex, "Error checking OpenWeatherAPI health");

            // Return unhealthy status if an exception occurs
            return HealthCheckResult.Unhealthy("Error checking OpenWeatherAPI health");
        }
    }
}
