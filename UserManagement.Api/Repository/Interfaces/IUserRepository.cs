using UserManagement.Api.Models;

namespace UserManagement.Api.Repository.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task<User?> UpdateAsync(Guid id, Action<User> updateAction);
    Task<bool> DeleteAsync(Guid id);
}
