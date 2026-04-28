using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Data;
using UserManagement.Api.Models;
using UserManagement.Api.Repository.Interfaces;

namespace UserManagement.Api.Repository;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await db.Users.Where(u => u.IsActive).ToListAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await db.Users.FindAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> CreateAsync(User user)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    public async Task<User?> UpdateAsync(Guid id, Action<User> updateAction)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null)
        {
            return null;
        }

        updateAction(user);
        await db.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null)
        {
            return false;
        }

        user.IsActive = false; // soft delete
        await db.SaveChangesAsync();
        return true;
    }
}
