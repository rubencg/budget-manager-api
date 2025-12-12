using BudgetManager.Service.Extensions;
using BudgetManager.Service.Services;

namespace BudgetManager.Service;

public class Program
{
    private const string _corsPolicyName = "AllowLocalhost3000";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new BudgetManager.Service.Infrastructure.Json.DecimalJsonConverter());
            });
        builder.Services.AddSwaggerDocumentation();
        builder.Services.AddCosmosDb(builder.Configuration, builder.Environment);
        builder.Services.AddRepositories();
        builder.Services.AddMediatrServices();
        builder.Services.AddUserContext();
        builder.Services.AddAuth0Authentication(builder.Configuration);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(_corsPolicyName,
                policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

        var app = builder.Build();

        // Configure middleware pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        if (app.Environment.IsDevelopment() &&
            !Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER")
                ?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
        {
            app.UseHttpsRedirection();
        }

        app.UseCors(_corsPolicyName);
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}