using CustomerManagementSystem.Domain.Entities;
using CustomerManagementSystem.Infrastructure.Services;

namespace CustomerManagementSystem.Application.Customers.Commands;

public class AddInteractionCommand
{
    public int CustomerId { get; set; }
    public DateTime? InteractionDate { get; set; }
    public InteractionType Type { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? PerformedBy { get; set; }
}

public class AddInteractionCommandHandler
{
    private readonly ICustomerService _customerService;

    public AddInteractionCommandHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public Task<Interaction> HandleAsync(AddInteractionCommand command, CancellationToken cancellationToken = default)
    {
        var interaction = new Interaction
        {
            CustomerId = command.CustomerId,
            InteractionDate = command.InteractionDate ?? DateTime.UtcNow,
            Type = command.Type,
            Subject = command.Subject,
            Details = command.Details,
            Summary = command.Subject,
            Notes = command.Details,
            PerformedBy = command.PerformedBy
        };

        return _customerService.AddInteractionAsync(interaction, cancellationToken);
    }
}

