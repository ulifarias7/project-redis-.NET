using UserManagement.Api.Dtos;
using UserManagement.Api.Models;

namespace UserManagement.Api.Services.Interfaces;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(Guid id);
    Task<User> CreateUserAsync(CreateUserDto dto);
    Task<User?> UpdateUserAsync(Guid id, UpdateUserDto dto);
    Task<bool> DeleteUserAsync(Guid id);
}
