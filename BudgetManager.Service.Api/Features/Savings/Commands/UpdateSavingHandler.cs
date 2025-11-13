using BudgetManager.Api.Domain;
using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using MediatR;

namespace BudgetManager.Service.Features.Savings.Commands;

public class UpdateSavingHandler : IRequestHandler<UpdateSavingCommand, Saving>
{
    private readonly ISavingRepository _savingRepository;
    private readonly ILogger<UpdateSavingHandler> _logger;

    public UpdateSavingHandler(
        ISavingRepository savingRepository,
        ILogger<UpdateSavingHandler> logger)
    {
        _savingRepository = savingRepository;
        _logger = logger;
    }

    public async Task<Saving> Handle(UpdateSavingCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Updating saving {SavingId} for user {UserId}",
            request.SavingId,
            request.UserId);

        var existingSaving = await _savingRepository.GetByIdAsync(request.SavingId, request.UserId, cancellationToken);

        if (existingSaving == null || existingSaving.ItemType != DomainConstants.SavingsType)
        {
            throw new InvalidOperationException($"Saving with ID {request.SavingId} not found");
        }

        var updatedSaving = new Saving
        {
            Id = request.SavingId,
            UserId = request.UserId,
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
