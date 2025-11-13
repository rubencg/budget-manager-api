using BudgetManager.Service.Infrastructure.Cosmos;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services;
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
        builder.Services.AddSwaggerGen();

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

        // Register repositories
        builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
        builder.Services.AddScoped<IAccountRepository, AccountRepository>();
        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
        builder.Services.AddScoped<IPlannedExpenseRepository, PlannedExpenseRepository>();
        builder.Services.AddScoped<ISavingRepository, SavingRepository>();

        builder.Services.AddMediatrServices();


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

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}