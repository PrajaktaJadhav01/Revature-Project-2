namespace CustomerManagementSystem.Api.Contracts.Auth;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public int? AssignedRepId { get; set; }
}

