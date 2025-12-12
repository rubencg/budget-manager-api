using BudgetManager.Api.Domain.Entities;
using BudgetManager.Service.Infrastructure.Pagination;
using MediatR;

namespace BudgetManager.Service.Features.Accounts.Queries;

public class GetArchivedAccountsQuery : IRequest<PagedResult<Account>>
{
    public PaginationParams Pagination { get; set; } = new();
    public SortParams Sort { get; set; } = new();
}
