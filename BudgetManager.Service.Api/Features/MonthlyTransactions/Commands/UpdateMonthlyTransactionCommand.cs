using BudgetManager.Api.Domain.Entities;
using BudgetManager.Api.Domain.Enums;
using MediatR;

namespace BudgetManager.Service.Features.MonthlyTransactions.Commands;

public record UpdateMonthlyTransactionCommand : CreateMonthlyTransactionCommand, IRequest<MonthlyTransaction>
{
    public string? Id { get; init; }
}
