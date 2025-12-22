using BudgetManager.Api.Domain.Entities;
using BudgetManager.Api.Domain.Enums;
using BudgetManager.Service.Infrastructure.Pagination;
using MediatR;

namespace BudgetManager.Service.Features.Transactions.Queries;

public record GetTransactionsByMonthQuery : IRequest<PagedResult<Transaction>>
{
    public int Year { get; init; }
    public int Month { get; init; }
    public TransactionType? TransactionType { get; init; }
    public string? CategoryId { get; init; }
    public string? SearchText { get; init; }
    public PaginationParams Pagination { get; init; } = new();
    public SortParams Sort { get; init; } = new();
}