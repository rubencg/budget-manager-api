using MediatR;

namespace BudgetManager.Service.Features.Accounts.Commands;

public record DeleteAccountCommand : IRequest
{
    public required string AccountId { get; init; }
}
