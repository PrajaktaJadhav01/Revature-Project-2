using CustomerManagementSystem.Infrastructure.Services;

namespace CustomerManagementSystem.Application.Customers.Commands;

public class DeleteContactCommand
{
    public int ContactId { get; set; }
}

public class DeleteContactCommandHandler
{
    private readonly ICustomerService _customerService;

    public DeleteContactCommandHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public Task<bool> HandleAsync(DeleteContactCommand command, CancellationToken cancellationToken = default)
    {
        return _customerService.DeleteContactAsync(command.ContactId, cancellationToken);
    }
}

