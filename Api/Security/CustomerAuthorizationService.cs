using System.Security.Claims;
using CustomerManagementSystem.Infrastructure.Services;

namespace CustomerManagementSystem.Api.Security;

public class CustomerAuthorizationService
{
    private readonly ICustomerService _customerService;

    public CustomerAuthorizationService(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<bool> CanAccessCustomerAsync(ClaimsPrincipal user, int customerId, CancellationToken cancellationToken = default)
    {
        var role = user.GetRole();
        if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "SalesManager", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(role, "SalesRep", StringComparison.OrdinalIgnoreCase))
        {
            var assignedRepId = user.GetAssignedRepId();
            var ownerRepId = await _customerService.GetAssignedSalesRepIdAsync(customerId, cancellationToken);
            return assignedRepId.HasValue && ownerRepId.HasValue && ownerRepId.Value == assignedRepId.Value;
        }

        return false;
    }
}

