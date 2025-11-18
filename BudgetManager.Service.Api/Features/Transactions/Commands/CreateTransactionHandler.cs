using BudgetManager.Api.Domain.Entities;
using BudgetManager.Api.Domain.Enums;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.AccountBalance;
using BudgetManager.Service.Services.UserContext;
using BudgetManager.Service.Services.Validation;
using MediatR;

namespace BudgetManager.Service.Features.Transactions.Commands;

public class CreateTransactionHandler : IRequestHandler<CreateTransactionCommand, Transaction>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionValidationService _transactionValidationService;
    private readonly IAccountBalanceService _accountBalanceService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateTransactionHandler> _logger;

    public CreateTransactionHandler(
        ITransactionRepository transactionRepository,
        ITransactionValidationService transactionValidationService,
        IAccountBalanceService accountBalanceService,
        ICurrentUserService currentUserService,
        ILogger<CreateTransactionHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _transactionValidationService = transactionValidationService;
        _accountBalanceService = accountBalanceService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Transaction> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Creating transaction for user {UserId}: Type={TransactionType}, Amount={Amount}",
            userId, request.TransactionType, request.Amount);

        // VALIDATIONS
        if (request.TransactionType == TransactionType.Transfer)
        {
            // For transfers, validate both FromAccountId and ToAccountId
            _transactionValidationService.ValidateTransferRules(
                request.FromAccountId,
                request.ToAccountId);

            await _transactionValidationService.ValidateTransferAccountsAsync(
                request.FromAccountId!,
                request.ToAccountId!,
                userId,
                cancellationToken);
        }
        else
        {
            // For non-transfer transactions, validate AccountId
            await _transactionValidationService.ValidateAccountExistsAsync(
                request.AccountId!,
                userId,
                cancellationToken);
        }

        // Validate monthly transaction rules
        _transactionValidationService.ValidateMonthlyTransactionRules(
            request.TransactionType,
            request.IsApplied);

        // Create transaction entity
        var transaction = new Transaction
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            TransactionType = request.TransactionType,
            Amount = request.Amount,
            Date = request.Date,

            // Account references
            // For ALL transaction types, set AccountId and AccountName
            AccountId = request.AccountId!,
            AccountName = request.AccountName!,

            // For TRANSFERS ONLY, also set FromAccountId/ToAccountId
            FromAccountId = request.TransactionType == TransactionType.Transfer
                ? request.FromAccountId
                : null,
            FromAccountName = request.TransactionType == TransactionType.Transfer
                ? request.FromAccountName
                : null,
            ToAccountId = request.TransactionType == TransactionType.Transfer
                ? request.ToAccountId
                : null,
            ToAccountName = request.TransactionType == TransactionType.Transfer
                ? request.ToAccountName
                : null,

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

            // Indexing fields (calculated from date)
            YearMonth = $"{request.Date.Year}-{request.Date.Month:D2}",
            Year = request.Date.Year,
            Month = request.Date.Month,
            Day = request.Date.Day,

            Metadata = request.Metadata
        };

        var createdTransaction = await _transactionRepository.CreateAsync(transaction, cancellationToken);

        _logger.LogInformation(
            "Successfully created transaction {TransactionId} for user {UserId}",
            createdTransaction.Id, userId);

        // APPLY BALANCE UPDATES
        if (ShouldApplyBalance(createdTransaction))
        {
            await _accountBalanceService.ApplyTransactionToBalanceAsync(
                createdTransaction,
                userId,
                cancellationToken);

            _logger.LogInformation(
                "Applied balance updates for transaction {TransactionId} of type {TransactionType}",
                createdTransaction.Id, createdTransaction.TransactionType);
        }

        return createdTransaction;
    }

    /// <summary>
    /// Determines if a transaction should apply to account balance.
    /// Transfers ALWAYS apply immediately (no IsApplied check).
    /// Other types only apply if IsApplied == true.
    /// </summary>
    private bool ShouldApplyBalance(Transaction transaction)
    {
        // Transfers always apply immediately
        if (transaction.TransactionType == TransactionType.Transfer)
            return true;

        // Other types only apply if IsApplied = true
        return transaction.IsApplied;
    }
}
