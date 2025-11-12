using BudgetManager.Api.Domain.Entities;
using MediatR;

namespace BudgetManager.Service.Features.Accounts.Commands;

public record UpdateAccountCommand : CreateAccountCommand, IRequest<Account>
{
    public required string AccountId { get; init; }
}
