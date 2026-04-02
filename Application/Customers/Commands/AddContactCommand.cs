using CustomerManagementSystem.Domain.Entities;
using CustomerManagementSystem.Infrastructure.Services;

namespace CustomerManagementSystem.Application.Customers.Commands;

public class AddContactCommand
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsPrimary { get; set; }
}

public class AddContactCommandHandler
{
    private readonly ICustomerService _customerService;

    public AddContactCommandHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public Task<Contact> HandleAsync(AddContactCommand command, CancellationToken cancellationToken = default)
    {
        var contact = new Contact
        {
            CustomerId = command.CustomerId,
            Name = command.Name,
            Title = command.Title,
            Email = command.Email,
            Phone = command.Phone,
            IsPrimary = command.IsPrimary
        };

        return _customerService.AddContactAsync(contact, cancellationToken);
    }
}

