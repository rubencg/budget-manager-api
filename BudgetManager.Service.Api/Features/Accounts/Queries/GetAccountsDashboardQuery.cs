using MediatR;

namespace BudgetManager.Service.Features.Accounts.Queries;

public class GetAccountsDashboardQuery : IRequest<List<AccountDashboardGroupDto>>
{
}
