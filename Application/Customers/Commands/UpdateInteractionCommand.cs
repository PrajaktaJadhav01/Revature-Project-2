using CustomerManagementSystem.Domain.Entities;
using CustomerManagementSystem.Infrastructure.Services;

namespace CustomerManagementSystem.Application.Customers.Commands;

public class UpdateInteractionCommand
{
    public int InteractionId { get; set; }
    public DateTime? InteractionDate { get; set; }
    public InteractionType Type { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? PerformedBy { get; set; }
}

public class UpdateInteractionCommandHandler
{
    private readonly ICustomerService _customerService;

    public UpdateInteractionCommandHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public Task<Interaction?> HandleAsync(UpdateInteractionCommand command, CancellationToken cancellationToken = default)
    {
        var interaction = new Interaction
        {
            InteractionId = command.InteractionId,
            InteractionDate = command.InteractionDate ?? default,
            Type = command.Type,
            Subject = command.Subject,
            Details = command.Details,
            Summary = command.Subject,
            Notes = command.Details,
            PerformedBy = command.PerformedBy
        };

        return _customerService.UpdateInteractionAsync(interaction, cancellationToken);
    }
}

