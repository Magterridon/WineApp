using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WineApp.Api.Data;
using WineApp.Api.DTOs;
using WineApp.Api.Models;

namespace WineApp.Api.Services;

public class AuthService(AppDbContext db, IConfiguration config)
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email))
            throw new InvalidOperationException("Email already in use");

        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };
        db.Users.Add(user);
        db.Cellars.Add(new Cellar { User = user });
        await db.SaveChangesAsync();

        return BuildResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        return BuildResponse(user);
    }

    private AuthResponse BuildResponse(User user) => new()
    {
        Token = GenerateToken(user),
        UserId = user.Id,
        Email = user.Email,
        Role = user.Role.ToString()
    };

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(config["Jwt:ExpiresInMinutes"] ?? "1440")),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
