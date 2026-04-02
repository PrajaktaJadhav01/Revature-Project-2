using CustomerManagementSystem.Infrastructure.Services;

namespace CustomerManagementSystem.Application.Customers.Commands;

public class DeleteCustomerCommand
{
    public int CustomerId { get; set; }
}

public class DeleteCustomerCommandHandler
{
    private readonly ICustomerService _customerService;

    public DeleteCustomerCommandHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public Task<bool> HandleAsync(DeleteCustomerCommand command, CancellationToken cancellationToken = default)
    {
        return _customerService.DeleteCustomerAsync(command.CustomerId, cancellationToken);
    }
}

