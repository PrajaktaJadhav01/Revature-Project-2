using CustomerManagementSystem.Domain.Entities;

namespace CustomerManagementSystem.Infrastructure.Services;

public interface IAuthService
{
    Task<AppUser?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
}

