using BudgetManager.Api.Domain.Entities;
using MediatR;

namespace BudgetManager.Service.Features.Transactions.Commands;

public record UpdateTransactionCommand : CreateTransactionCommand, IRequest<Transaction>
{
    public required string TransactionId { get; init; }
}
