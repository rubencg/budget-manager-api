namespace BudgetManager.Service.Infrastructure.Cosmos;

public class CosmosDbSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string TransactionsContainer { get; set; } = null!;
    public string AccountsContainer { get; set; } = null!;
    public string CategoriesContainer { get; set; } = null!;
    public string PlannedItemsContainer { get; set; } = null!;
    public string UsersContainer { get; set; } = null!;
}
