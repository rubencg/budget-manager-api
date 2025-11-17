using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.Transactions.Commands;

public class CreateTransactionHandler : IRequestHandler<CreateTransactionCommand, Transaction>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateTransactionHandler> _logger;

    public CreateTransactionHandler(
        ITransactionRepository transactionRepository,
        ICurrentUserService currentUserService,
        ILogger<CreateTransactionHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Transaction> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Creating transaction for user {UserId}: Type={TransactionType}, Amount={Amount}",
            userId, request.TransactionType, request.Amount);

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

        return createdTransaction;
    }
}
