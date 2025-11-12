using BudgetManager.Api.Domain.Entities;
using MediatR;

namespace BudgetManager.Service.Features.Transactions.Queries;

public record GetTransactionByIdQuery : IRequest<Transaction?>
{
    public required string TransactionId { get; init; }
    public required string UserId { get; init; }
}
