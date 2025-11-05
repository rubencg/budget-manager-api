namespace BudgetManager.Service.Services;

public static class MediatrServices
{
    public static void AddMediatrServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
    }
}