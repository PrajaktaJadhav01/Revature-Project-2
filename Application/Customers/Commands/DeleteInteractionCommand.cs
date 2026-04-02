using CustomerManagementSystem.Infrastructure.Services;

namespace CustomerManagementSystem.Application.Customers.Commands;

public class DeleteInteractionCommand
{
    public int InteractionId { get; set; }
}

public class DeleteInteractionCommandHandler
{
    private readonly ICustomerService _customerService;

    public DeleteInteractionCommandHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public Task<bool> HandleAsync(DeleteInteractionCommand command, CancellationToken cancellationToken = default)
    {
        return _customerService.DeleteInteractionAsync(command.InteractionId, cancellationToken);
    }
}

