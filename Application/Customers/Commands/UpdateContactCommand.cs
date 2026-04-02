using CustomerManagementSystem.Domain.Entities;
using CustomerManagementSystem.Infrastructure.Services;

namespace CustomerManagementSystem.Application.Customers.Commands;

public class UpdateContactCommand
{
    public int ContactId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsPrimary { get; set; }
}

public class UpdateContactCommandHandler
{
    private readonly ICustomerService _customerService;

    public UpdateContactCommandHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public Task<Contact?> HandleAsync(UpdateContactCommand command, CancellationToken cancellationToken = default)
    {
        var contact = new Contact
        {
            ContactId = command.ContactId,
            Name = command.Name,
            Title = command.Title,
            Email = command.Email,
            Phone = command.Phone,
            IsPrimary = command.IsPrimary
        };

        return _customerService.UpdateContactAsync(contact, cancellationToken);
    }
}

