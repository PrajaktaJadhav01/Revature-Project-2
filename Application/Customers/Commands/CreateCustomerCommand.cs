using CustomerManagementSystem.Domain.Entities;
using CustomerManagementSystem.Infrastructure.Services;

namespace CustomerManagementSystem.Application.Customers.Commands;

public class CreateCustomerCommand
{
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? Industry { get; set; }
    public string? CompanySize { get; set; }
    public CustomerClassification Classification { get; set; }
    public CustomerType Type { get; set; }
    public CustomerSegment Segment { get; set; }
    public decimal AccountValue { get; set; }
    public int? AssignedSalesRepId { get; set; }
}

public class CreateCustomerCommandHandler
{
    private readonly ICustomerService _customerService;

    public CreateCustomerCommandHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public Task<Customer> HandleAsync(CreateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        var customer = new Customer
        {
            CustomerName = command.CustomerName,
            Email = command.Email,
            Phone = command.Phone,
            Website = command.Website,
            Industry = command.Industry,
            CompanySize = command.CompanySize,
            Classification = command.Classification,
            Type = command.Type,
            Segment = command.Segment,
            AccountValue = command.AccountValue,
            AssignedSalesRepId = command.AssignedSalesRepId
        };

        return _customerService.CreateCustomerAsync(customer, cancellationToken);
    }
}

