using System.ComponentModel.DataAnnotations;

namespace CustomerManagementSystem.Api.Contracts.Customers;

public class AddContactRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Title { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [RegularExpression(@"^[\d\+\-\s\(\)]+$", ErrorMessage = "Invalid phone format.")]
    public string? Phone { get; set; }

    public bool IsPrimary { get; set; }
}

public class UpdateContactRequest : AddContactRequest
{
}

