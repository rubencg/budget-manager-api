using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using MediatR;

namespace BudgetManager.Service.Features.Transactions.Commands;

public class DeleteTransactionHandler : IRequestHandler<DeleteTransactionCommand, Unit>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<DeleteTransactionHandler> _logger;

    public DeleteTransactionHandler(
        ITransactionRepository transactionRepository,
        ILogger<DeleteTransactionHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Deleting transaction {TransactionId} for user {UserId}",
            request.TransactionId, request.UserId);

        await _transactionRepository.DeleteAsync(
            request.TransactionId,
            request.UserId,
            cancellationToken);

        _logger.LogInformation(
            "Successfully deleted transaction {TransactionId} for user {UserId}",
            request.TransactionId, request.UserId);

        return Unit.Value;
    }
}
