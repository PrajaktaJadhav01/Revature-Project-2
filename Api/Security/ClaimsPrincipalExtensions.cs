using System.Security.Claims;

namespace CustomerManagementSystem.Api.Security;

public static class ClaimsPrincipalExtensions
{
    public static string? GetRole(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Role);
    }

    public static int? GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue("userId");
        return int.TryParse(value, out var id) ? id : null;
    }

    public static int? GetAssignedRepId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue("assignedRepId");
        return int.TryParse(value, out var id) ? id : null;
    }
}

