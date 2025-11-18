using BudgetManager.Service.Infrastructure.Cosmos;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace BudgetManager.Service.Extensions;

/// <summary>
/// Extension methods for configuring Cosmos DB services.
/// </summary>
public static class CosmosDbServiceExtensions
{
    /// <summary>
    /// Adds Cosmos DB client and configuration to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environment">The web host environment.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCosmosDb(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.Configure<CosmosDbSettings>(
            configuration.GetSection("CosmosDb"));

        services.AddSingleton<CosmosClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<CosmosDbSettings>>().Value;
            var logger = sp.GetRequiredService<ILogger<Program>>();

            var cosmosClientOptions = new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Gateway,
                LimitToEndpoint = true,

                // Retry policy configuration for transient errors
                MaxRetryAttemptsOnRateLimitedRequests = 5,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30),

                // Performance optimizations
                EnableContentResponseOnWrite = false, // Reduces RU on write operations
                RequestTimeout = TimeSpan.FromSeconds(30),

                // Consistency level (Session is default, but explicit is better)
                ConsistencyLevel = ConsistencyLevel.Session
            };

            // Only disable certificate validation for localhost in Development
            // This is required for the Cosmos DB emulator which uses a self-signed certificate
            if (environment.IsDevelopment() &&
                settings.ConnectionString.Contains("localhost", StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning(
                    "Disabling SSL certificate validation for Cosmos DB localhost connection. " +
                    "This should ONLY be used with the local emulator in development.");

                cosmosClientOptions.HttpClientFactory = () =>
                {
                    HttpMessageHandler httpMessageHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                    };
                    return new HttpClient(httpMessageHandler);
                };
            }
            // Production and non-localhost environments use default (secure) certificate validation

            return new CosmosClient(settings.ConnectionString, cosmosClientOptions);
        });

        return services;
    }
}
