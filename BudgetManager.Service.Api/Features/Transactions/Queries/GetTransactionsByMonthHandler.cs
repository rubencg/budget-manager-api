using BudgetManager.Api.Domain.Entities;
using MediatR;

namespace BudgetManager.Service.Features.Transactions.Queries;

public class GetTransactionsByMonthQueryHandler
    : IRequestHandler<GetTransactionsByMonthQuery, GetTransactionsByMonthQueryResult>
{
    public async Task<GetTransactionsByMonthQueryResult> Handle(
        GetTransactionsByMonthQuery request,
        CancellationToken cancellationToken)
    {
        var yearMonth = $"{request.Year}-{request.Month:D2}";
        
        return new GetTransactionsByMonthQueryResult(
            yearMonth,
            1,
            500,
            1000,
            new List<Transaction>()
            {
                new()
                {
                    AccountName = "Ruben Debito",
                    Month = 10,
                    Year = 2025,
                    Day = 27,
                    Amount = 500
                }
            }
        );
    }
}