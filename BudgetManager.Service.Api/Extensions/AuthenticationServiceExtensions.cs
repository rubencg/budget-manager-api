using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace BudgetManager.Service.Extensions;

/// <summary>
/// Extension methods for configuring authentication services.
/// </summary>
public static class AuthenticationServiceExtensions
{
    /// <summary>
    /// Adds Auth0 JWT authentication to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when Auth0 configuration is missing.</exception>
    public static IServiceCollection AddAuth0Authentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var auth0Domain = configuration["Auth0:Domain"]
            ?? Environment.GetEnvironmentVariable("AUTH0_DOMAIN")
            ?? throw new InvalidOperationException("Auth0:Domain is not configured. Please set it in appsettings.json or AUTH0_DOMAIN environment variable.");

        var auth0Audience = configuration["Auth0:Audience"]
            ?? Environment.GetEnvironmentVariable("AUTH0_AUDIENCE")
            ?? throw new InvalidOperationException("Auth0:Audience is not configured. Please set it in appsettings.json or AUTH0_AUDIENCE environment variable.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.Authority = $"https://{auth0Domain}/";
            options.Audience = auth0Audience;

            // Configure token validation and caching
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5) // Allow 5 minutes clock skew
            };

            // JWKS (public keys) are cached automatically
            // By default: RefreshInterval = 24 hours, AutomaticRefreshInterval = 12 hours
            // This allows offline operation once keys are cached

            // Handle authentication events with structured logging
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();

                    logger.LogWarning(context.Exception,
                        "Authentication failed for {Path}. Reason: {Reason}",
                        context.Request.Path,
                        context.Exception.Message);

                    // Add custom response headers for debugging (development only)
                    if (context.HttpContext.RequestServices
                        .GetRequiredService<IWebHostEnvironment>().IsDevelopment())
                    {
                        context.Response.Headers.Append("X-Auth-Failure-Reason",
                            context.Exception.GetType().Name);
                    }

                    return Task.CompletedTask;
                },

                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();

                    var userId = context.Principal?.FindFirst("sub")?.Value;
                    logger.LogInformation(
                        "Token validated successfully for user {UserId}", userId ?? "Unknown");

                    return Task.CompletedTask;
                },

                OnChallenge = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();

                    logger.LogWarning(
                        "Authentication challenge for {Path}. Error: {Error}, Description: {Description}",
                        context.Request.Path,
                        context.Error ?? "None",
                        context.ErrorDescription ?? "None");

                    // Customize challenge response
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";

                    var environment = context.HttpContext.RequestServices
                        .GetRequiredService<IWebHostEnvironment>();

                    var result = JsonSerializer.Serialize(new
                    {
                        error = "unauthorized",
                        message = "Authentication required. Please provide a valid Bearer token.",
                        details = environment.IsDevelopment()
                            ? context.ErrorDescription
                            : null
                    });

                    return context.Response.WriteAsync(result);
                },

                OnForbidden = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();

                    var userId = context.Principal?.FindFirst("sub")?.Value;
                    logger.LogWarning(
                        "User {UserId} forbidden from accessing {Path}",
                        userId ?? "Unknown",
                        context.Request.Path);

                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }
}
