namespace BudgetManager.Service.Features.Savings.Commands;

public record UpdateSavingCommand : CreateSavingCommand
{
    public string? SavingId { get; init; }
}
