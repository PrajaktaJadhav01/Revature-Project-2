using System.ComponentModel.DataAnnotations;
using CustomerManagementSystem.Domain.Entities;

namespace CustomerManagementSystem.Api.Contracts.Customers;

public class AddAddressRequest
{
    [Required]
    public AddressType AddressType { get; set; }

    [Required]
    public string Street { get; set; } = string.Empty;

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string State { get; set; } = string.Empty;

    [Required]
    public string PostalCode { get; set; } = string.Empty;

    [Required]
    public string Country { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }
}

public class UpdateAddressRequest : AddAddressRequest
{
}

