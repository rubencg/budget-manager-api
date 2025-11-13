using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using MediatR;

namespace BudgetManager.Service.Features.PlannedExpenses.Commands;

public class DeletePlannedExpenseHandler : IRequestHandler<DeletePlannedExpenseCommand>
{
    private readonly IPlannedExpenseRepository _plannedExpenseRepository;
    private readonly ILogger<DeletePlannedExpenseHandler> _logger;

    public DeletePlannedExpenseHandler(
        IPlannedExpenseRepository plannedExpenseRepository,
        ILogger<DeletePlannedExpenseHandler> logger)
    {
        _plannedExpenseRepository = plannedExpenseRepository;
        _logger = logger;
    }

    public async Task Handle(DeletePlannedExpenseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Deleting planned expense {PlannedExpenseId} for user {UserId}",
            request.PlannedExpenseId,
            request.UserId);

        await _plannedExpenseRepository.DeleteAsync(
            request.PlannedExpenseId,
            request.UserId,
            cancellationToken);
    }
}
