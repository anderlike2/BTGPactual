using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Application.DTOs.Responses;

namespace BTGPactual.Application.Services.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserResponse>> GetAllUsersAsync();
    Task<UserResponse> GetUserByIdAsync(string userId);
    Task<UserResponse> UpdateUserAsync(string userId, UpdateUserRequest request);
}