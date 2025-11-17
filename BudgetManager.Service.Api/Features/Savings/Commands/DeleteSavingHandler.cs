using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.Savings.Commands;

public class DeleteSavingHandler : IRequestHandler<DeleteSavingCommand>
{
    private readonly ISavingRepository _savingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteSavingHandler> _logger;

    public DeleteSavingHandler(
        ISavingRepository savingRepository,
        ICurrentUserService currentUserService,
        ILogger<DeleteSavingHandler> logger)
    {
        _savingRepository = savingRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task Handle(DeleteSavingCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Deleting saving {SavingId} for user {UserId}",
            request.SavingId,
            userId);

        await _savingRepository.DeleteAsync(request.SavingId, userId, cancellationToken);
    }
}
