using CustomerManagementSystem.Domain.Entities;
using CustomerManagementSystem.Infrastructure.Services;

namespace CustomerManagementSystem.Application.Customers.Queries;

public class GetAllCustomersQuery
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? AssignedSalesRepId { get; set; }
}

public class GetAllCustomersResult
{
    public IReadOnlyList<Customer> Items { get; set; } = Array.Empty<Customer>();
    public int TotalCount { get; set; }
}

public class GetAllCustomersQueryHandler
{
    private readonly ICustomerService _customerService;

    public GetAllCustomersQueryHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<GetAllCustomersResult> HandleAsync(GetAllCustomersQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _customerService.GetCustomersAsync(query.PageNumber, query.PageSize, query.AssignedSalesRepId, cancellationToken);
        return new GetAllCustomersResult
        {
            Items = items,
            TotalCount = totalCount
        };
    }
}

