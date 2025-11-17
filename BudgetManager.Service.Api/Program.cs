using BudgetManager.Service.Infrastructure.Cosmos;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services;
using BudgetManager.Service.Services.UserContext;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;

namespace BudgetManager.Service;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            // Define the Bearer security scheme
            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Enter your token in the text input below."
            });

            // Require Bearer token for all endpoints
            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // Configure Cosmos DB
        builder.Services.Configure<CosmosDbSettings>(
            builder.Configuration.GetSection("CosmosDb"));

        builder.Services.AddSingleton<CosmosClient>(sp =>
        {
            var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CosmosDbSettings>>().Value;

            var cosmosClientOptions = new CosmosClientOptions
            {
                HttpClientFactory = () =>
                {
                    HttpMessageHandler httpMessageHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                    };

                    return new HttpClient(httpMessageHandler);
                },
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

            return new CosmosClient(settings.ConnectionString, cosmosClientOptions);
        });

        // Register HTTP context accessor (required for ICurrentUserService)
        builder.Services.AddHttpContextAccessor();

        // Register user context service
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Register repositories
        builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
        builder.Services.AddScoped<IAccountRepository, AccountRepository>();
        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
        builder.Services.AddScoped<IPlannedExpenseRepository, PlannedExpenseRepository>();
        builder.Services.AddScoped<ISavingRepository, SavingRepository>();

        builder.Services.AddMediatrServices();

        // Configure Auth0 from appsettings.json
        var auth0Domain = builder.Configuration["Auth0:Domain"]
            ?? Environment.GetEnvironmentVariable("AUTH0_DOMAIN")
            ?? throw new InvalidOperationException("Auth0:Domain is not configured");
        
        var auth0Audience = builder.Configuration["Auth0:Audience"]
            ?? Environment.GetEnvironmentVariable("AUTH0_AUDIENCE")
            ?? throw new InvalidOperationException("Auth0:Audience is not configured");

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.Authority = $"https://{auth0Domain}/";
            options.Audience = auth0Audience;

            // Configure token validation and caching
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
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
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Only use HTTPS redirection in Development when running locally
        if (app.Environment.IsDevelopment() && !Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
        {
            app.UseHttpsRedirection();
        }

        // Enable authentication middleware (must come before authorization)
        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}