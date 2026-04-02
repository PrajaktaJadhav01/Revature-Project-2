namespace CustomerManagementSystem.Domain.Entities;

public enum CustomerClassification
{
    Prospect,
    Active,
    Inactive,
    VIP,
    AtRisk
}

public enum CustomerType
{
    Business,
    Individual
}

public enum CustomerSegment
{
    Enterprise,
    MidMarket,
    SMB
}

public class Customer
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

    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<Interaction> Interactions { get; set; } = new List<Interaction>();
}
