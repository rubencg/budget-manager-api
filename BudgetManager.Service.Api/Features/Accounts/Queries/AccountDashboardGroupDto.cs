using BudgetManager.Api.Domain.Entities;

namespace BudgetManager.Service.Features.Accounts.Queries;

public class AccountDashboardGroupDto
{
    public string GroupName { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<Account> Accounts { get; set; } = new();
}
