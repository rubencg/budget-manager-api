using BudgetManager.Service.Extensions;
using BudgetManager.Service.Services;

namespace BudgetManager.Service;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services
        builder.Services.AddControllers();
        builder.Services.AddSwaggerDocumentation();
        builder.Services.AddCosmosDb(builder.Configuration, builder.Environment);
        builder.Services.AddRepositories();
        builder.Services.AddMediatrServices();
        builder.Services.AddUserContext();
        builder.Services.AddAuth0Authentication(builder.Configuration);

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

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}