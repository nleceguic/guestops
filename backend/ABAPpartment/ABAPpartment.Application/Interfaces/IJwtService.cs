using ABAPpartment.Domain.Entities;

namespace ABAPpartment.Application.Interfaces;

public interface IJwtService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}

