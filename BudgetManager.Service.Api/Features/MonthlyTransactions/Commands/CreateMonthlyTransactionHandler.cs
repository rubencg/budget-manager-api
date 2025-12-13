using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using BudgetManager.Service.Services.Validation;
using MediatR;

namespace BudgetManager.Service.Features.MonthlyTransactions.Commands;

public class CreateMonthlyTransactionHandler : IRequestHandler<CreateMonthlyTransactionCommand, MonthlyTransaction>
{
    private readonly IMonthlyTransactionRepository _repository;
    private readonly ITransactionValidationService _validationService; // Reusing transaction validation for generic account checks
    private readonly ICurrentUserService _currentUserService;

    public CreateMonthlyTransactionHandler(
        IMonthlyTransactionRepository repository,
        ITransactionValidationService validationService,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _validationService = validationService;
        _currentUserService = currentUserService;
    }

    public async Task<MonthlyTransaction> Handle(CreateMonthlyTransactionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        // Validate Account
        await _validationService.ValidateAccountExistsAsync(request.AccountId, userId, cancellationToken);

        // Validate Day of Month
        if (request.DayOfMonth < 1 || request.DayOfMonth > 31)
        {
            throw new InvalidOperationException("Day of month must be between 1 and 31");
        }

        var monthlyTransaction = new MonthlyTransaction
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Amount = request.Amount,
            MonthlyTransactionType = request.MonthlyTransactionType,
            Notes = request.Notes,
            DayOfMonth = request.DayOfMonth,
            AccountId = request.AccountId,
            AccountName = request.AccountName,
            CategoryId = request.CategoryId,
            CategoryName = request.CategoryName,
            Subcategory = request.Subcategory,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _repository.CreateAsync(monthlyTransaction, cancellationToken);
    }
}
