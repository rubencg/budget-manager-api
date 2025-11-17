using BudgetManager.Api.Domain.Entities;
using MediatR;

namespace BudgetManager.Service.Features.Accounts.Queries;

public record GetAccountByIdQuery : IRequest<Account?>
{
    public required string AccountId { get; init; }
}
