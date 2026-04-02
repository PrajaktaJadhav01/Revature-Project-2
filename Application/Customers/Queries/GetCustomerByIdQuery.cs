using CustomerManagementSystem.Domain.Entities;
using CustomerManagementSystem.Infrastructure.Services;

namespace CustomerManagementSystem.Application.Customers.Queries;

public class GetCustomerByIdQuery
{
    public int CustomerId { get; set; }
}

public class GetCustomerByIdQueryHandler
{
    private readonly ICustomerService _customerService;

    public GetCustomerByIdQueryHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public Task<Customer?> HandleAsync(GetCustomerByIdQuery query, CancellationToken cancellationToken = default)
    {
        return _customerService.GetCustomerByIdAsync(query.CustomerId, cancellationToken);
    }
}

