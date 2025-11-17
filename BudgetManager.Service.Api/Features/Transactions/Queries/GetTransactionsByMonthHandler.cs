using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using BudgetManager.Service.Services.UserContext;
using MediatR;

namespace BudgetManager.Service.Features.Transactions.Queries;

public class GetTransactionsByMonthQueryHandler
    : IRequestHandler<GetTransactionsByMonthQuery, GetTransactionsByMonthQueryResult>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetTransactionsByMonthQueryHandler(
        ITransactionRepository transactionRepository,
        ICurrentUserService currentUserService)
    {
        _transactionRepository = transactionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetTransactionsByMonthQueryResult> Handle(
        GetTransactionsByMonthQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var yearMonth = $"{request.Year}-{request.Month:D2}";

        var transactions = await _transactionRepository.GetByMonthAsync(userId, yearMonth
            , cancellationToken: cancellationToken);

        return new GetTransactionsByMonthQueryResult(
            yearMonth,
            1,
            500,
            1000,
            transactions
        );
    }
}