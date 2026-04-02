namespace CustomerManagementSystem.Domain.Entities;

public enum InteractionType
{
    Call,
    Email,
    SupportTicket,
    Meeting,
    Other
}

public class Interaction
{
    public int InteractionId { get; set; }
    public int CustomerId { get; set; }
    public DateTime InteractionDate { get; set; }
    public InteractionType Type { get; set; }
    // PDF fields
    public string Subject { get; set; } = string.Empty;
    public string? Details { get; set; }

    // Back-compat fields
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string Summary { get; set; } = string.Empty;
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? Notes { get; set; }
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? PerformedBy { get; set; }

    public Customer? Customer { get; set; }
}
