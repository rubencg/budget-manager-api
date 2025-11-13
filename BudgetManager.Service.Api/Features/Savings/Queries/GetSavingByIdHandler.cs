using BudgetManager.Api.Domain;
using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using MediatR;

namespace BudgetManager.Service.Features.Savings.Queries;

public class GetSavingByIdHandler : IRequestHandler<GetSavingByIdQuery, Saving?>
{
    private readonly ISavingRepository _savingRepository;
    private readonly ILogger<GetSavingByIdHandler> _logger;

    public GetSavingByIdHandler(
        ISavingRepository savingRepository,
        ILogger<GetSavingByIdHandler> logger)
    {
        _savingRepository = savingRepository;
        _logger = logger;
    }

    public async Task<Saving?> Handle(GetSavingByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting saving {SavingId} for user {UserId}",
            request.SavingId,
            request.UserId);

        var saving = await _savingRepository.GetByIdAsync(request.SavingId, request.UserId, cancellationToken);
        return saving?.ItemType != DomainConstants.SavingsType ? null : saving;
    }
}
