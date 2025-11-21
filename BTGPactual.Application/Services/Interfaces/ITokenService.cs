using BTGPactual.Domain.Entities;

namespace BTGPactual.Application.Services.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}