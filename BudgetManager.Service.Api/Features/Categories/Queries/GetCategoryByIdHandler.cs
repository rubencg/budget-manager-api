using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using MediatR;

namespace BudgetManager.Service.Features.Categories.Queries;

public class GetCategoryByIdHandler : IRequestHandler<GetCategoryByIdQuery, Category?>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<GetCategoryByIdHandler> _logger;

    public GetCategoryByIdHandler(
        ICategoryRepository categoryRepository,
        ILogger<GetCategoryByIdHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<Category?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting category {CategoryId} for user {UserId}",
            request.CategoryId,
            request.UserId);

        return await _categoryRepository.GetByIdAsync(
            request.CategoryId,
            request.UserId,
            cancellationToken);
    }
}
