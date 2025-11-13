using MediatR;

namespace BudgetManager.Service.Features.Savings.Commands;

public record DeleteSavingCommand : IRequest
{
    public required string SavingId { get; init; }
    public required string UserId { get; init; }
}
