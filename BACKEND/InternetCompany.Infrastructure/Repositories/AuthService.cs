using InternetCompany.Application.DTOs.Auth;
using InternetCompany.Application.Interfaces;
using InternetCompany.Domain.Entities;
using InternetCompany.Infrastructure.Persistence;
using InternetCompany.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InternetCompany.Infrastructure.Repositories;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Status)
            .FirstOrDefaultAsync(u =>
                u.Username == request.Username &&
                !u.IsDeleted);

        if (user == null)
            throw new Exception("Usuario no encontrado");

        if (!PasswordHasher.Verify(request.Password, user.PasswordHash))
            throw new Exception("Contraseña incorrecta");

        if (user.Status.Code != "ACT")
            throw new Exception("Usuario no activo");

        var token = GenerateJwtToken(user);

        return new LoginResponseDto
        {
            Token = token,
            Username = user.Username,
            Role = user.Role.Name
        };
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.Name),
            new Claim("UserId", user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                double.Parse(jwtSettings["DurationInMinutes"]!)),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
