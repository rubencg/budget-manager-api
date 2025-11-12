using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using MediatR;

namespace BudgetManager.Service.Features.Transactions.Queries;

public class GetTransactionByIdHandler : IRequestHandler<GetTransactionByIdQuery, Transaction?>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<GetTransactionByIdHandler> _logger;

    public GetTransactionByIdHandler(
        ITransactionRepository transactionRepository,
        ILogger<GetTransactionByIdHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    public async Task<Transaction?> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting transaction {TransactionId} for user {UserId}",
            request.TransactionId, request.UserId);

        var transaction = await _transactionRepository.GetByIdAsync(
            request.TransactionId,
            request.UserId,
            cancellationToken);

        if (transaction == null)
        {
            _logger.LogWarning(
                "Transaction {TransactionId} not found for user {UserId}",
                request.TransactionId, request.UserId);
        }

        return transaction;
    }
}
