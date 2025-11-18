using BudgetManager.Api.Domain.Entities;
using BudgetManager.Api.Domain.Enums;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.AccountBalance;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.Transactions.Commands;

public class CreateTransactionHandler : IRequestHandler<CreateTransactionCommand, Transaction>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountBalanceService _accountBalanceService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateTransactionHandler> _logger;

    public CreateTransactionHandler(
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        IAccountBalanceService accountBalanceService,
        ICurrentUserService currentUserService,
        ILogger<CreateTransactionHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
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
        await ValidateAccountExistsAsync(request.AccountId, userId, cancellationToken);
        ValidateTransferRules(request);
        ValidateMonthlyTransactionRules(request);

        // Create transaction entity
        var transaction = new Transaction
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            TransactionType = request.TransactionType,
            Amount = request.Amount,
            Date = request.Date,

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

    /// <summary>
    /// Validates that the account exists and is not archived.
    /// </summary>
    private async Task ValidateAccountExistsAsync(
        string accountId,
        string userId,
        CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(accountId, userId, cancellationToken);

        if (account == null)
        {
            _logger.LogError(
                "Account {AccountId} not found for user {UserId}",
                accountId, userId);
            throw new InvalidOperationException($"Account {accountId} not found");
        }

        if (account.IsArchived)
        {
            _logger.LogError(
                "Cannot create transaction for archived account {AccountId}",
                accountId);
            throw new InvalidOperationException(
                $"Cannot create transactions for archived account {account.Name}");
        }
    }

    /// <summary>
    /// Validates transfer-specific business rules.
    /// </summary>
    private void ValidateTransferRules(CreateTransactionCommand request)
    {
        if (request.TransactionType != TransactionType.Transfer)
            return;

        // ToAccountId is required for transfers
        if (string.IsNullOrEmpty(request.ToAccountId))
        {
            _logger.LogError("Transfer transaction is missing ToAccountId");
            throw new InvalidOperationException("ToAccountId is required for transfer transactions");
        }

        // Cannot transfer to same account
        if (request.AccountId == request.ToAccountId)
        {
            _logger.LogError(
                "Transfer transaction has same source and destination account: {AccountId}",
                request.AccountId);
            throw new InvalidOperationException("Cannot transfer to the same account");
        }

        // Note: We don't validate ToAccountId existence here because we want to keep
        // the validation lightweight. The AccountBalanceService will validate this
        // when applying the balance update.
    }

    /// <summary>
    /// Validates monthly transaction business rules.
    /// MonthlyExpense and MonthlyIncome must be created with IsApplied=false.
    /// </summary>
    private void ValidateMonthlyTransactionRules(CreateTransactionCommand request)
    {
        if (request.TransactionType == TransactionType.MonthlyExpense && request.IsApplied)
        {
            _logger.LogError(
                "Attempted to create MonthlyExpense with IsApplied=true");
            throw new InvalidOperationException(
                "MonthlyExpense transactions must be created with IsApplied=false");
        }

        if (request.TransactionType == TransactionType.MonthlyIncome && request.IsApplied)
        {
            _logger.LogError(
                "Attempted to create MonthlyIncome with IsApplied=true");
            throw new InvalidOperationException(
                "MonthlyIncome transactions must be created with IsApplied=false");
        }
    }
}
