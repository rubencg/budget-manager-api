using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.Categories.Queries;

public class GetCategoriesByTypeHandler : IRequestHandler<GetCategoriesByTypeQuery, List<Category>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetCategoriesByTypeHandler> _logger;

    public GetCategoriesByTypeHandler(
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService,
        ILogger<GetCategoriesByTypeHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<List<Category>> Handle(GetCategoriesByTypeQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Getting categories of type {CategoryType} for user {UserId}",
            request.CategoryType,
            userId);

        return await _categoryRepository.GetByCategoryTypeAsync(
            userId,
            request.CategoryType,
            cancellationToken);
    }
}
