using BudgetManager.Api.Domain.Entities;
using MediatR;

namespace BudgetManager.Service.Features.Categories.Queries;

public record GetCategoryByIdQuery : IRequest<Category?>
{
    public required string CategoryId { get; init; }
    public required string UserId { get; init; }
}
