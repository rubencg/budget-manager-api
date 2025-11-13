using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using MediatR;

namespace BudgetManager.Service.Features.PlannedExpenses.Queries;

public class GetPlannedExpenseByIdHandler : IRequestHandler<GetPlannedExpenseByIdQuery, PlannedExpense?>
{
    private readonly IPlannedExpenseRepository _plannedExpenseRepository;
    private readonly ILogger<GetPlannedExpenseByIdHandler> _logger;

    public GetPlannedExpenseByIdHandler(
        IPlannedExpenseRepository plannedExpenseRepository,
        ILogger<GetPlannedExpenseByIdHandler> logger)
    {
        _plannedExpenseRepository = plannedExpenseRepository;
        _logger = logger;
    }

    public async Task<PlannedExpense?> Handle(GetPlannedExpenseByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting planned expense {PlannedExpenseId} for user {UserId}",
            request.PlannedExpenseId,
            request.UserId);

        return await _plannedExpenseRepository.GetByIdAsync(
            request.PlannedExpenseId,
            request.UserId,
            cancellationToken);
    }
}
