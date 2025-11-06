using BudgetManager.Service.Services;

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