using MediatR;

namespace BudgetManager.Service.Features.Accounts.Commands;

public record DeleteAccountCommand : IRequest
{
    public required string AccountId { get; init; }
    public required string UserId { get; init; }
}
