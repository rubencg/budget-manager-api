using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using MediatR;

namespace BudgetManager.Service.Features.Savings.Commands;

public class DeleteSavingHandler : IRequestHandler<DeleteSavingCommand>
{
    private readonly ISavingRepository _savingRepository;
    private readonly ILogger<DeleteSavingHandler> _logger;

    public DeleteSavingHandler(
        ISavingRepository savingRepository,
        ILogger<DeleteSavingHandler> logger)
    {
        _savingRepository = savingRepository;
        _logger = logger;
    }

    public async Task Handle(DeleteSavingCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Deleting saving {SavingId} for user {UserId}",
            request.SavingId,
            request.UserId);

        await _savingRepository.DeleteAsync(request.SavingId, request.UserId, cancellationToken);
    }
}
