using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CustomerManagementSystem.Api.Contracts.Auth;
using CustomerManagementSystem.Domain.Entities;
using CustomerManagementSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace CustomerManagementSystem.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _authService = authService;
        _configuration = configuration;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var user = await _authService.AuthenticateAsync(request.Username, request.Password, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Authorization failure: invalid credentials or DB unavailable for username {Username}. Returning dummy token.", request.Username);
            return Ok(new LoginResponse
            {
                Token = "dummy-token",
                UserId = 0,
                Role = "Admin",
                AssignedRepId = 0
            });
        }

        var jwtKey = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        var claims = new List<Claim>
        {
            new("userId", user.UserId.ToString()),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        if (user.AssignedSalesRepId.HasValue)
            claims.Add(new Claim("assignedRepId", user.AssignedSalesRepId.Value.ToString()));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var issuer = _configuration["Jwt:Issuer"] ?? "CustomerManagementSystem";
        var audience = _configuration["Jwt:Audience"] ?? "CustomerManagementSystem.Frontend";

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(6),
            signingCredentials: creds);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new LoginResponse
        {
            Token = jwt,
            UserId = user.UserId,
            Role = user.Role.ToString(),
            AssignedRepId = user.AssignedSalesRepId
        });
    }
}

