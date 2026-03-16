using ABAPpartment.Application.DTOs.Auth;
using ABAPpartment.Application.Interfaces;
using ABAPpartment.Domain.Entities;
using ABAPpartment.Domain.Interfaces;

namespace ABAPpartment.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IJwtService _jwt;

    public AuthService(IUserRepository users, IJwtService jwt)
    {
        _users = users;
        _jwt = jwt;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req, CancellationToken ct = default)
    {
        if (await _users.ExistsByEmailAsync(req.Email, ct))
            throw new InvalidOperationException($"El email '{req.Email}' ya está registrado.");

        if (!UserRole.IsValid(req.Role))
            throw new ArgumentException($"Rol inválido: {req.Role}.");

        var user = new User
        {
            Email = req.Email.Trim().ToLowerInvariant(),
            FirstName = req.FirstName.Trim(),
            LastName = req.LastName.Trim(),
            Phone = req.Phone?.Trim(),
            Role = req.Role,
            Language = req.Language,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        await _users.AddAsync(user, ct);
        await _users.SaveChangesAsync(ct);

        var (token, expiresAt) = _jwt.GenerateToken(user);
        return new AuthResponse(token, expiresAt, ToDto(user));
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(req.Email.Trim().ToLowerInvariant(), ct)
            ?? throw new UnauthorizedAccessException("Email o contraseña incorrectos.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("La cuenta está desactivada.");

        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email o contraseña incorrectos.");

        user.LastLoginAt = DateTime.UtcNow;
        _users.Update(user);
        await _users.SaveChangesAsync(ct);

        var (token, expiresAt) = _jwt.GenerateToken(user);
        return new AuthResponse(token, expiresAt, ToDto(user));
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest req, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException($"Usuario {userId} no encontrado.");

        if (!BCrypt.Net.BCrypt.Verify(req.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("La contraseña actual es incorrecta.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        _users.Update(user);
        await _users.SaveChangesAsync(ct);
    }

    public async Task<UserDto> GetProfileAsync(int userId, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException($"Usuario {userId} no encontrado.");

        return ToDto(user);
    }

    private static UserDto ToDto(User u) => new(
        u.Id, u.Email, u.FirstName, u.LastName,
        u.Phone, u.Role, u.Language, u.IsActive, u.CreatedAt
    );
}