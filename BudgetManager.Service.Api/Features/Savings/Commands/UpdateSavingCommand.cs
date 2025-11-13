namespace BudgetManager.Service.Features.Savings.Commands;

public record UpdateSavingCommand : CreateSavingCommand
{
    public required string SavingId { get; init; }
}
