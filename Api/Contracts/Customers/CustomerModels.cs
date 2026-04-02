using System.ComponentModel.DataAnnotations;
using CustomerManagementSystem.Domain.Entities;

namespace CustomerManagementSystem.Api.Contracts.Customers;

public class CreateCustomerRequest
{
    [Required]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [RegularExpression(@"^[\d\+\-\s\(\)]+$", ErrorMessage = "Invalid phone format.")]
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

public class UpdateCustomerRequest
{
    [Required]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [RegularExpression(@"^[\d\+\-\s\(\)]+$", ErrorMessage = "Invalid phone format.")]
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

public class CustomerResponse
{
    public int CustomerId { get; set; }
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
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public int? AssignedSalesRepId { get; set; }

    public ICollection<ContactResponse> Contacts { get; set; } = new List<ContactResponse>();
    public ICollection<AddressResponse> Addresses { get; set; } = new List<AddressResponse>();
    public ICollection<InteractionResponse> Interactions { get; set; } = new List<InteractionResponse>();
}

public class ChangeClassificationRequest
{
    public CustomerClassification Classification { get; set; }
}

public class CustomersPageResponse
{
    public IReadOnlyList<CustomerResponse> Items { get; set; } = Array.Empty<CustomerResponse>();
    public int TotalCount { get; set; }
}

