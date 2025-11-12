using BudgetManager.Api.Domain.Entities;
using BudgetManager.Api.Domain.Enums;
using MediatR;

namespace BudgetManager.Service.Features.Categories.Commands;

public record CreateCategoryCommand : IRequest<Category>
{
    public required string UserId { get; init; }
    public required string Name { get; init; }
    public required string Image { get; init; }
    public string? Color { get; init; }
    public CategoryType CategoryType { get; init; }
    public List<string>? Subcategories { get; init; }
}
