using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using MediatR;

namespace BudgetManager.Service.Features.PlannedExpenses.Commands;

public class CreatePlannedExpenseHandler : IRequestHandler<CreatePlannedExpenseCommand, PlannedExpense>
{
    private readonly IPlannedExpenseRepository _plannedExpenseRepository;
    private readonly ILogger<CreatePlannedExpenseHandler> _logger;

    public CreatePlannedExpenseHandler(
        IPlannedExpenseRepository plannedExpenseRepository,
        ILogger<CreatePlannedExpenseHandler> logger)
    {
        _plannedExpenseRepository = plannedExpenseRepository;
        _logger = logger;
    }

    public async Task<PlannedExpense> Handle(CreatePlannedExpenseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating planned expense {PlannedExpenseName} for user {UserId}",
            request.Name,
            request.UserId);

        var plannedExpense = new PlannedExpense
        {
            Id = Guid.NewGuid().ToString(),
            UserId = request.UserId,
            ItemType = "plannedExpense",
            Name = request.Name,
            Date = request.Date,
            IsRecurring = request.IsRecurring,
            TotalAmount = request.TotalAmount,
            CategoryId = request.CategoryId,
            CategoryName = request.CategoryName,
            CategoryImage = request.CategoryImage,
            CategoryColor = request.CategoryColor,
            SubCategory = request.SubCategory
        };

        return await _plannedExpenseRepository.CreateAsync(plannedExpense, cancellationToken);
    }
}
