namespace CustomerManagementSystem.Domain.Entities;

public enum AddressType
{
    Billing,
    Shipping,
    Primary
}

public class Address
{
    public int AddressId { get; set; }
    public int CustomerId { get; set; }
    // PDF fields
    public AddressType AddressType { get; set; }
    public string Street { get; set; } = string.Empty;

    // Back-compat fields (still used by current API DTO mapping)
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string Line1 { get; set; } = string.Empty;
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }

    public Customer? Customer { get; set; }
}
