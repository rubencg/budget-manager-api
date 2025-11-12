using BudgetManager.Api.Domain.Entities;
using BudgetManager.Api.Domain.Enums;
using MediatR;

namespace BudgetManager.Service.Features.Accounts.Commands;

public record CreateAccountCommand : IRequest<Account>
{
    public required string UserId { get; init; }
    public required string Name { get; init; }
    public decimal CurrentBalance { get; init; }
    public AccountType AccountType { get; init; }
    public string? Color { get; init; }
    public string? Image { get; init; }
    public bool IsArchived { get; init; } = false;
}
