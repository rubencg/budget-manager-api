using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.PlannedExpenses.Commands;

public class CreatePlannedExpenseHandler : IRequestHandler<CreatePlannedExpenseCommand, PlannedExpense>
{
    private readonly IPlannedExpenseRepository _plannedExpenseRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreatePlannedExpenseHandler> _logger;

    public CreatePlannedExpenseHandler(
        IPlannedExpenseRepository plannedExpenseRepository,
        ICurrentUserService currentUserService,
        ILogger<CreatePlannedExpenseHandler> logger)
    {
        _plannedExpenseRepository = plannedExpenseRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<PlannedExpense> Handle(CreatePlannedExpenseCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Creating planned expense {PlannedExpenseName} for user {UserId}",
            request.Name,
            userId);

        var plannedExpense = new PlannedExpense
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Name = request.Name,
            Date = request.Date,
            DayOfMonth = request.DayOfMonth,
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
