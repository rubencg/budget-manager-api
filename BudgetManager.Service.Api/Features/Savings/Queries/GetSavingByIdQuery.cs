using BudgetManager.Api.Domain.Entities;
using MediatR;

namespace BudgetManager.Service.Features.Savings.Queries;

public record GetSavingByIdQuery : IRequest<Saving?>
{
    public required string SavingId { get; init; }
    public required string UserId { get; init; }
}
