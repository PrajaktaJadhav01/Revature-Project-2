using CustomerManagementSystem.Domain.Entities;
using CustomerManagementSystem.Infrastructure.Services;

namespace CustomerManagementSystem.Application.Customers.Commands;

public class ChangeClassificationCommand
{
    public int CustomerId { get; set; }
    public CustomerClassification Classification { get; set; }
}

public class ChangeClassificationCommandHandler
{
    private readonly ICustomerService _customerService;

    public ChangeClassificationCommandHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public Task<bool> HandleAsync(ChangeClassificationCommand command, CancellationToken cancellationToken = default)
    {
        return _customerService.ChangeClassificationAsync(command.CustomerId, command.Classification, cancellationToken);
    }
}

