using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using BudgetManager.Service.Services.Validation;
using MediatR;

namespace BudgetManager.Service.Features.MonthlyTransactions.Commands;

public class UpdateMonthlyTransactionHandler : IRequestHandler<UpdateMonthlyTransactionCommand, MonthlyTransaction>
{
    private readonly IMonthlyTransactionRepository _repository;
    private readonly ITransactionValidationService _validationService;
    private readonly ICurrentUserService _currentUserService;

    public UpdateMonthlyTransactionHandler(
        IMonthlyTransactionRepository repository,
        ITransactionValidationService validationService,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _validationService = validationService;
        _currentUserService = currentUserService;
    }

    public async Task<MonthlyTransaction> Handle(UpdateMonthlyTransactionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (request.Id == null)
        {
            throw new ArgumentNullException(nameof(request.Id), "Transaction ID cannot be null");
        }

        var existingTransaction = await _repository.GetByIdAsync(request.Id, userId, cancellationToken);
        if (existingTransaction == null)
        {
            throw new KeyNotFoundException($"Monthly transaction {request.Id} not found");
        }

        // Validate Account
        await _validationService.ValidateAccountExistsAsync(request.AccountId, userId, cancellationToken);

        // Validate Day of Month
        if (request.DayOfMonth < 1 || request.DayOfMonth > 31)
        {
            throw new InvalidOperationException("Day of month must be between 1 and 31");
        }

        existingTransaction.Amount = request.Amount;
        existingTransaction.MonthlyTransactionType = request.MonthlyTransactionType;
        existingTransaction.Notes = request.Notes;
        existingTransaction.DayOfMonth = request.DayOfMonth;
        existingTransaction.AccountId = request.AccountId;
        existingTransaction.AccountName = request.AccountName;
        existingTransaction.CategoryId = request.CategoryId;
        existingTransaction.CategoryName = request.CategoryName;
        existingTransaction.Subcategory = request.Subcategory;
        existingTransaction.UpdatedAt = DateTime.UtcNow;

        return await _repository.UpdateAsync(existingTransaction, cancellationToken);
    }
}
