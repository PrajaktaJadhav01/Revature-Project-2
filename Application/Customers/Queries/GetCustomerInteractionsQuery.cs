using CustomerManagementSystem.Domain.Entities;
using CustomerManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagementSystem.Application.Customers.Queries;

public class GetCustomerInteractionsQuery
{
    public int CustomerId { get; set; }
}

public class GetCustomerInteractionsQueryHandler
{
    private readonly AppDbContext _dbContext;

    public GetCustomerInteractionsQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Interaction>> HandleAsync(GetCustomerInteractionsQuery query, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Interactions
            .Where(i => i.CustomerId == query.CustomerId)
            .OrderByDescending(i => i.InteractionDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}

