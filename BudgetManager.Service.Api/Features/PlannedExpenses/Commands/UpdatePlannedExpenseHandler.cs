using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using MediatR;

namespace BudgetManager.Service.Features.PlannedExpenses.Commands;

public class UpdatePlannedExpenseHandler : IRequestHandler<UpdatePlannedExpenseCommand, PlannedExpense>
{
    private readonly IPlannedExpenseRepository _plannedExpenseRepository;
    private readonly ILogger<UpdatePlannedExpenseHandler> _logger;

    public UpdatePlannedExpenseHandler(
        IPlannedExpenseRepository plannedExpenseRepository,
        ILogger<UpdatePlannedExpenseHandler> logger)
    {
        _plannedExpenseRepository = plannedExpenseRepository;
        _logger = logger;
    }

    public async Task<PlannedExpense> Handle(UpdatePlannedExpenseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Updating planned expense {PlannedExpenseId} for user {UserId}",
            request.PlannedExpenseId,
            request.UserId);

        // Get existing planned expense to preserve CreatedAt
        var existingPlannedExpense = await _plannedExpenseRepository.GetByIdAsync(
            request.PlannedExpenseId,
            request.UserId,
            cancellationToken);

        if (existingPlannedExpense == null)
        {
            throw new InvalidOperationException($"PlannedExpense {request.PlannedExpenseId} not found");
        }

        // Update planned expense with new values
        existingPlannedExpense.Name = request.Name;
        existingPlannedExpense.Date = request.Date;
        existingPlannedExpense.IsRecurring = request.IsRecurring;
        existingPlannedExpense.TotalAmount = request.TotalAmount;
        existingPlannedExpense.CategoryId = request.CategoryId;
        existingPlannedExpense.CategoryName = request.CategoryName;
        existingPlannedExpense.CategoryImage = request.CategoryImage;
        existingPlannedExpense.CategoryColor = request.CategoryColor;
        existingPlannedExpense.SubCategory = request.SubCategory;

        return await _plannedExpenseRepository.UpdateAsync(existingPlannedExpense, cancellationToken);
    }
}
