using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Application.DTOs.Responses;

namespace BTGPactual.Application.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}