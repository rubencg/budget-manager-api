using BudgetManager.Api.Domain;
using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.Savings.Commands;

public class UpdateSavingHandler : IRequestHandler<UpdateSavingCommand, Saving>
{
    private readonly ISavingRepository _savingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateSavingHandler> _logger;

    public UpdateSavingHandler(
        ISavingRepository savingRepository,
        ICurrentUserService currentUserService,
        ILogger<UpdateSavingHandler> logger)
    {
        _savingRepository = savingRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Saving> Handle(UpdateSavingCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Updating saving {SavingId} for user {UserId}",
            request.SavingId,
            userId);

        var existingSaving = await _savingRepository.GetByIdAsync(request.SavingId!, userId, cancellationToken);

        if (existingSaving == null || existingSaving.ItemType != DomainConstants.SavingsType)
        {
            throw new InvalidOperationException($"Saving with ID {request.SavingId!} not found");
        }

        var updatedSaving = new Saving
        {
            Id = request.SavingId!,
            UserId = userId,
            Name = request.Name,
            Icon = request.Icon,
            Color = request.Color,
            GoalAmount = request.GoalAmount,
            SavedAmount = request.SavedAmount,
            AmountPerMonth = request.AmountPerMonth,
            CreatedAt = existingSaving.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        return await _savingRepository.UpdateAsync(updatedSaving, cancellationToken);
    }
}
