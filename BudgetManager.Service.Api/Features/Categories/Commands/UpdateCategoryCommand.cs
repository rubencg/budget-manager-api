using BudgetManager.Api.Domain.Entities;
using MediatR;

namespace BudgetManager.Service.Features.Categories.Commands;

public record UpdateCategoryCommand : CreateCategoryCommand, IRequest<Category>
{
    public string? CategoryId { get; init; }
}
