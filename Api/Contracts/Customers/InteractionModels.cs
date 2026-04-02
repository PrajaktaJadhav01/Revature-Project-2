using System.ComponentModel.DataAnnotations;
using CustomerManagementSystem.Domain.Entities;

namespace CustomerManagementSystem.Api.Contracts.Customers;

public class AddInteractionRequest
{
    public DateTime? InteractionDate { get; set; }
    public InteractionType Type { get; set; }

    [Required]
    public string Subject { get; set; } = string.Empty;

    public string? Details { get; set; }
    public string? PerformedBy { get; set; }
}

public class UpdateInteractionRequest : AddInteractionRequest
{
}

