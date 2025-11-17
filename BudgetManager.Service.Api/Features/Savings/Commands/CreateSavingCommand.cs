using BudgetManager.Api.Domain.Entities;
using MediatR;

namespace BudgetManager.Service.Features.Savings.Commands;

public record CreateSavingCommand : IRequest<Saving>
{
    public required string Name { get; init; }
    public string? Icon { get; init; }
    public string? Color { get; init; }
    public decimal GoalAmount { get; init; }
    public decimal SavedAmount { get; init; }
    public decimal AmountPerMonth { get; init; }
}
