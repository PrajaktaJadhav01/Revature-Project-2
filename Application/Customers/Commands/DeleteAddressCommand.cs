using CustomerManagementSystem.Infrastructure.Services;

namespace CustomerManagementSystem.Application.Customers.Commands;

public class DeleteAddressCommand
{
    public int AddressId { get; set; }
}

public class DeleteAddressCommandHandler
{
    private readonly ICustomerService _customerService;

    public DeleteAddressCommandHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public Task<bool> HandleAsync(DeleteAddressCommand command, CancellationToken cancellationToken = default)
    {
        return _customerService.DeleteAddressAsync(command.AddressId, cancellationToken);
    }
}

