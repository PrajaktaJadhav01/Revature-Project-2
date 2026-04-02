using CustomerManagementSystem.Domain.Entities;

namespace CustomerManagementSystem.Api.Contracts.Customers;

public class ContactResponse
{
    public int ContactId { get; set; }
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsPrimary { get; set; }
}

public class AddressResponse
{
    public int AddressId { get; set; }
    public int CustomerId { get; set; }
    public AddressType AddressType { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

public class InteractionResponse
{
    public int InteractionId { get; set; }
    public int CustomerId { get; set; }
    public DateTime InteractionDate { get; set; }
    public InteractionType Type { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? PerformedBy { get; set; }
}

