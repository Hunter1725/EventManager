using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Data.SqlClient;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(IConfiguration configuration, ILogger<DatabaseHealthCheck> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        try
        {
            // Perform a query to check the health of the database
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync(cancellationToken);

                // Execute a query to verify database health
                var query = "SELECT 1";
                using (var command = new SqlCommand(query, connection))
                {
                    var result = await command.ExecuteScalarAsync(cancellationToken);

                    // Check if the query result is valid
                    if (result != null && result.ToString() == "1")
                    {
                        // Return healthy status if the query is successful
                        return HealthCheckResult.Healthy("Database connection and query are healthy");
                    }
                    else
                    {
                        // Return unhealthy status if the query result is not as expected
                        return HealthCheckResult.Unhealthy("Database query did not return the expected result");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log any exceptions that occur during the health check
            _logger.LogError(ex, "Error checking database health");

            // Return unhealthy status if an exception occurs
            return HealthCheckResult.Unhealthy("Database connection is unhealthy");
        }
    }
}
