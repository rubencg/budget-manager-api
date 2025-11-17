using BudgetManager.Api.Domain;
using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.Savings.Queries;

public class GetSavingByIdHandler : IRequestHandler<GetSavingByIdQuery, Saving?>
{
    private readonly ISavingRepository _savingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetSavingByIdHandler> _logger;

    public GetSavingByIdHandler(
        ISavingRepository savingRepository,
        ICurrentUserService currentUserService,
        ILogger<GetSavingByIdHandler> logger)
    {
        _savingRepository = savingRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Saving?> Handle(GetSavingByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Getting saving {SavingId} for user {UserId}",
            request.SavingId,
            userId);

        var saving = await _savingRepository.GetByIdAsync(request.SavingId, userId, cancellationToken);
        return saving?.ItemType != DomainConstants.SavingsType ? null : saving;
    }
}
