using MediatR;

namespace BudgetManager.Service.Features.Categories.Commands;

public record DeleteCategoryCommand : IRequest
{
    public required string CategoryId { get; init; }
}
