using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using MediatR;

namespace BudgetManager.Service.Features.Transactions.Commands;

public class UpdateTransactionHandler : IRequestHandler<UpdateTransactionCommand, Transaction>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<UpdateTransactionHandler> _logger;

    public UpdateTransactionHandler(
        ITransactionRepository transactionRepository,
        ILogger<UpdateTransactionHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    public async Task<Transaction> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Updating transaction {TransactionId} for user {UserId}",
            request.TransactionId, request.UserId);

        // Get existing transaction to preserve CreatedAt
        var existingTransaction = await _transactionRepository.GetByIdAsync(
            request.TransactionId,
            request.UserId,
            cancellationToken);

        if (existingTransaction == null)
        {
            _logger.LogError(
                "Transaction {TransactionId} not found for user {UserId}",
                request.TransactionId, request.UserId);
            throw new InvalidOperationException($"Transaction {request.TransactionId} not found");
        }

        // Update transaction entity
        var transaction = new Transaction
        {
            Id = request.TransactionId,
            UserId = request.UserId,
            TransactionType = request.TransactionType,
            Amount = request.Amount,
            Date = request.Date,
            CreatedAt = existingTransaction.CreatedAt, // Preserve original creation date

            // Account references
            AccountId = request.AccountId,
            AccountName = request.AccountName,
            ToAccountId = request.ToAccountId,
            ToAccountName = request.ToAccountName,

            // Category references
            CategoryId = request.CategoryId,
            CategoryName = request.CategoryName,
            CategoryImage = request.CategoryImage,
            CategoryColor = request.CategoryColor,
            Subcategory = request.Subcategory,

            // Transaction details
            Notes = request.Notes,
            IsApplied = request.IsApplied,
            AppliedAmount = request.AppliedAmount,

            // Monthly/recurring
            IsMonthly = request.IsMonthly,
            IsRecurring = request.IsRecurring,
            MonthlyKey = request.MonthlyKey,
            RecurringTimes = request.RecurringTimes,
            RecurringType = request.RecurringType,

            // References
            SavingKey = request.SavingKey,
            TransferId = request.TransferId,
            RemoveFromSpendingPlan = request.RemoveFromSpendingPlan,

            // Recalculate indexing fields from new date
            YearMonth = $"{request.Date.Year}-{request.Date.Month:D2}",
            Year = request.Date.Year,
            Month = request.Date.Month,
            Day = request.Date.Day,

            Metadata = request.Metadata
        };

        var updatedTransaction = await _transactionRepository.UpdateAsync(transaction, cancellationToken);

        _logger.LogInformation(
            "Successfully updated transaction {TransactionId} for user {UserId}",
            request.TransactionId, request.UserId);

        return updatedTransaction;
    }
}
