using BudgetManager.Service.Infrastructure.Cosmos.Repositories;
using MediatR;

namespace BudgetManager.Service.Features.Transactions.Queries;

public class GetTransactionsByMonthQueryHandler
    : IRequestHandler<GetTransactionsByMonthQuery, GetTransactionsByMonthQueryResult>
{
    private readonly ITransactionRepository _transactionRepository;

    public GetTransactionsByMonthQueryHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }
    
    public async Task<GetTransactionsByMonthQueryResult> Handle(
        GetTransactionsByMonthQuery request,
        CancellationToken cancellationToken)
    {
        var yearMonth = $"{request.Year}-{request.Month:D2}";
        var userId = request.AccountId;

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