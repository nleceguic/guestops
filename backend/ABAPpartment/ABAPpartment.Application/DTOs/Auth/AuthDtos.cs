namespace ABAPpartment.Application.DTOs.Auth;

/// <summary>Datos para registrar un nuevo usuario.</summary>

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? Phone = null,
    string Role = "Guest",
    string Language = "es"
);

/// <summary>Datos para iniciar sesión.</summary>

public record LoginRequest(
    string Email,
    string Password
);

/// <summary>Datos para cambiar la contraseña (usuario autenticado).</summary>

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);

/// <summary>Respuesta tras login o registro exitoso.</summary>

public record AuthResponse(
    string AccessToken,
    DateTime ExpiresAt,
    UserDto User
);

/// <summary>Datos públicos del usuario (nunca exponer PasswordHash).</summary>

public record UserDto(
    int Id,
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    string Role,
    string Language,
    bool IsActive,
    DateTime CreatedAt
);