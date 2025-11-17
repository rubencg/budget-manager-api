using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.PlannedExpenses.Commands;

public class DeletePlannedExpenseHandler : IRequestHandler<DeletePlannedExpenseCommand>
{
    private readonly IPlannedExpenseRepository _plannedExpenseRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeletePlannedExpenseHandler> _logger;

    public DeletePlannedExpenseHandler(
        IPlannedExpenseRepository plannedExpenseRepository,
        ICurrentUserService currentUserService,
        ILogger<DeletePlannedExpenseHandler> logger)
    {
        _plannedExpenseRepository = plannedExpenseRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task Handle(DeletePlannedExpenseCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Deleting planned expense {PlannedExpenseId} for user {UserId}",
            request.PlannedExpenseId,
            userId);

        await _plannedExpenseRepository.DeleteAsync(
            request.PlannedExpenseId,
            userId,
            cancellationToken);
    }
}
