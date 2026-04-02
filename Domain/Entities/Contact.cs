namespace CustomerManagementSystem.Domain.Entities;

public class Contact
{
    // PDF naming: ContactPersonId. Kept as ContactId for DB/EF stability.
    public int ContactId { get; set; }
    public int CustomerId { get; set; }

    // Computed property; not mapped directly to DB.
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string Name
    {
        get => string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName)
            ? string.Empty
            : $"{FirstName} {LastName}".Trim();
        set
        {
            // write semantics are by-first-last from API mapping, not directly stored as one column
            if (!string.IsNullOrWhiteSpace(value))
            {
                var parts = value.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                FirstName = parts.Length > 0 ? parts[0] : string.Empty;
                LastName = parts.Length > 1 ? parts[1] : string.Empty;
            }
        }
    }

    public string? Title { get; set; }

    // Back-compat fields (still used by current API DTO mapping)
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsPrimary { get; set; }

    public Customer? Customer { get; set; }
}
