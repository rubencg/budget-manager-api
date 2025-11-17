using MediatR;

namespace BudgetManager.Service.Features.Transactions.Commands;

public record DeleteTransactionCommand : IRequest<Unit>
{
    public required string TransactionId { get; init; }
}
