using System.Security.Cryptography;
using System.Text;
using CustomerManagementSystem.Domain.Entities;
using CustomerManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagementSystem.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;

    public AuthService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AppUser?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
        if (user is null) return null;

        if (!PasswordVerifier.Verify(password, user.PasswordHash))
            return null;

        return user;
    }

    private static class PasswordVerifier
    {
        // Hash format: pbkdf2$iterations$saltBase64$hashBase64
        public static bool Verify(string password, string storedHash)
        {
            try
            {
                var parts = storedHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 4) return false;

                var algorithm = parts[0];
                if (!string.Equals(algorithm, "pbkdf2", StringComparison.OrdinalIgnoreCase)) return false;

                var iterations = int.Parse(parts[1]);
                var salt = Convert.FromBase64String(parts[2]);
                var stored = Convert.FromBase64String(parts[3]);

                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
                var computed = pbkdf2.GetBytes(stored.Length);
                return CryptographicOperations.FixedTimeEquals(stored, computed);
            }
            catch
            {
                return false;
            }
        }
    }
}

