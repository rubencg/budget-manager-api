using BudgetManager.Api.Domain;
using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.PlannedExpenses.Queries;

public class GetPlannedExpenseByIdHandler : IRequestHandler<GetPlannedExpenseByIdQuery, PlannedExpense?>
{
    private readonly IPlannedExpenseRepository _plannedExpenseRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetPlannedExpenseByIdHandler> _logger;

    public GetPlannedExpenseByIdHandler(
        IPlannedExpenseRepository plannedExpenseRepository,
        ICurrentUserService currentUserService,
        ILogger<GetPlannedExpenseByIdHandler> logger)
    {
        _plannedExpenseRepository = plannedExpenseRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<PlannedExpense?> Handle(GetPlannedExpenseByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Getting planned expense {PlannedExpenseId} for user {UserId}",
            request.PlannedExpenseId,
            userId);

        var plannedExpense = await _plannedExpenseRepository.GetByIdAsync(
            request.PlannedExpenseId,
            userId,
            cancellationToken);

        return plannedExpense?.ItemType != DomainConstants.PlannedExpensesType ? null : plannedExpense;
    }
}
