using CustomerManagementSystem.Domain.Entities;
using CustomerManagementSystem.Infrastructure.Services;

namespace CustomerManagementSystem.Application.Customers.Commands;

public class AddAddressCommand
{
    public int CustomerId { get; set; }
    public AddressType AddressType { get; set; }
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

public class AddAddressCommandHandler
{
    private readonly ICustomerService _customerService;

    public AddAddressCommandHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public Task<Address> HandleAsync(AddAddressCommand command, CancellationToken cancellationToken = default)
    {
        var address = new Address
        {
            CustomerId = command.CustomerId,
            AddressType = command.AddressType,
            Line1 = command.Line1,
            Line2 = command.Line2,
            Street = command.Line1,
            City = command.City,
            State = command.State,
            PostalCode = command.PostalCode,
            Country = command.Country,
            IsPrimary = command.IsPrimary
        };

        return _customerService.AddAddressAsync(address, cancellationToken);
    }
}

