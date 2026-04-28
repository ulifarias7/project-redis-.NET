using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using UserManagement.Api.Dtos;
using UserManagement.Api.Models;
using UserManagement.Api.Repository.Interfaces;
using UserManagement.Api.Services.Interfaces;

namespace UserManagement.Api.Services;

public class UserService(
    IUserRepository repository,
    IDistributedCache cache) : IUserService
{
    // La key en Redis será: "usermgmt:user:<guid>"
    // (el prefijo "usermgmt:" lo agrega InstanceName en Program.cs)
    private static string CacheKey(Guid id) => $"user:{id}"; //los valores en redis como dicccionarios clave > valor

    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    };

    public Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return repository.GetAllAsync();
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        var key = CacheKey(id);

        var cached = await cache.GetStringAsync(key);
        if (cached is not null)
        {
            return JsonSerializer.Deserialize<User>(cached);
        }
        var user = await repository.GetByIdAsync(id);

        if (user is not null)
        {
            await cache.SetStringAsync(key, JsonSerializer.Serialize(user), CacheOptions);
        }

        return user;
    }

    public async Task<User> CreateUserAsync(CreateUserDto dto)
    {
        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(dto.Password)))
        };

        return await repository.CreateAsync(user);
    }

    public async Task<User?> UpdateUserAsync(Guid id, UpdateUserDto dto)
    {
        var updated = await repository.UpdateAsync(id, user =>
        {
            if (dto.Name  is not null) user.Name  = dto.Name;
            if (dto.Email is not null) user.Email = dto.Email;
        });

        if (updated is not null)
        {
            var key = CacheKey(id);
            await cache.RemoveAsync(key);
        }

        return updated;
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var deleted = await repository.DeleteAsync(id);
        if (deleted)
        {
            var key = CacheKey(id);
            await cache.RemoveAsync(key);
        }

        return deleted;
    }
}
