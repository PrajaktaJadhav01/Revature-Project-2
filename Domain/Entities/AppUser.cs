namespace CustomerManagementSystem.Domain.Entities;

public enum UserRole
{
    SalesRep,
    SalesManager,
    Admin
}

public class AppUser
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    /// <summary>
    /// For SalesRep accounts, this can be used to scope assigned customers.
    /// </summary>
    public int? AssignedSalesRepId { get; set; }
}
