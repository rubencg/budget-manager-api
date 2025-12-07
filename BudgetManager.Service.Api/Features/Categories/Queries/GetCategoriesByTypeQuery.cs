using BudgetManager.Api.Domain.Entities;
using BudgetManager.Api.Domain.Enums;
using MediatR;

namespace BudgetManager.Service.Features.Categories.Queries;

public record GetCategoriesByTypeQuery : IRequest<List<Category>>
{
    public required CategoryType CategoryType { get; init; }
}
