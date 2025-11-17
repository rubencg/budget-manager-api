using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.Transactions.Queries;

public class GetTransactionByIdHandler : IRequestHandler<GetTransactionByIdQuery, Transaction?>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetTransactionByIdHandler> _logger;

    public GetTransactionByIdHandler(
        ITransactionRepository transactionRepository,
        ICurrentUserService currentUserService,
        ILogger<GetTransactionByIdHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Transaction?> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Getting transaction {TransactionId} for user {UserId}",
            request.TransactionId, userId);

        var transaction = await _transactionRepository.GetByIdAsync(
            request.TransactionId,
            userId,
            cancellationToken);

        if (transaction == null)
        {
            _logger.LogWarning(
                "Transaction {TransactionId} not found for user {UserId}",
                request.TransactionId, userId);
        }

        return transaction;
    }
}
