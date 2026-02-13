using InternetCompany.Application.Common.Exceptions;
using InternetCompany.Application.DTOs.Users;
using InternetCompany.Application.Interfaces;
using InternetCompany.Domain.Entities;
using InternetCompany.Infrastructure.Persistence;
using InternetCompany.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserResponseDto> CreateAsync(CreateUserDto dto, int currentUserId)
    {
        ValidateUsername(dto.Username);
        ValidatePassword(dto.Password);

        if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            throw new BusinessException("El username ya existe.");

        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            throw new BusinessException("El email ya está registrado.");

        var currentUser = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == currentUserId);

        if (currentUser == null)
            throw new BusinessException("Usuario actual no encontrado.");

        bool autoApprove = currentUser.Role.Name == "Admin";

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = PasswordHasher.Hash(dto.Password),
            RoleId = dto.RoleId,
            StatusId = 1,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            IsApproved = autoApprove,
            ApprovedByUserId = autoApprove ? currentUserId : null,
            ApprovedAt = autoApprove ? DateTime.UtcNow : null
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var role = await _context.Roles.FindAsync(user.RoleId);

        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = role!.Name,
            IsApproved = user.IsApproved
        };
    }


    public async Task ApproveAsync(int userId, int adminId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            throw new BusinessException("Usuario no encontrado");

        user.IsApproved = true;
        user.ApprovedByUserId = adminId;
        user.ApprovedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Role)
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role.Name,
                IsApproved = u.IsApproved
            })
            .ToListAsync();
    }

    private void ValidateUsername(string username)
    {
        if (username.Length < 8 || username.Length > 20)
            throw new BusinessException("El username debe tener entre 8 y 20 caracteres.");

        if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,20}$"))
            throw new BusinessException("El username debe contener letras y al menos un número, sin caracteres especiales.");
    }

    private void ValidatePassword(string password)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"^(?=.*[A-Z])(?=.*\d).{8,30}$"))
            throw new BusinessException("La contraseña debe tener mínimo 8 caracteres, al menos una mayúscula y un número.");
    }
}
