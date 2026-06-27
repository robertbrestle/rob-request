using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RobRequest.Shared.Data;
using RobRequest.Shared.Models;

namespace RobRequest.Shared.Services;

public class UserService(AppDbContext db, IPasswordHasher<User> passwordHasher)
{
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await db.Users
            .Include(u => u.Group)
            .OrderBy(u => u.Username)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<User?> GetUserAsync(string id)
    {
        return await db.Users
            .Include(u => u.Group)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<List<UserGroup>> GetAllGroupsAsync()
    {
        return await db.UserGroups
            .OrderBy(g => g.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<(bool Success, string? Error)> CreateUserAsync(string username, string password, string groupId)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return (false, "Username and password are required.");

        if (password.Length < 8)
            return (false, "Password must be at least 8 characters.");

        var exists = await db.Users.AnyAsync(u => u.Username == username);
        if (exists)
            return (false, "Username is already taken.");

        var group = await db.UserGroups.FindAsync(groupId);
        if (group == null)
            return (false, "Invalid group.");

        var user = new User
        {
            Username = username,
            GroupId = groupId,
            IsEnabled = true,
            IsApproved = true // Admin-created users are pre-approved
        };
        user.PasswordHash = passwordHasher.HashPassword(user, password);

        db.Users.Add(user);

        // Create default settings for the new user
        var settings = new UserSettings
        {
            UserId = user.Id
        };
        db.UserSettings.Add(settings);

        await db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null) return false;

        db.Users.Remove(user);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleEnabledAsync(string id)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null) return false;

        user.IsEnabled = !user.IsEnabled;
        user.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ApproveUserAsync(string id)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null) return false;

        user.IsApproved = true;
        user.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<string?> ResetPasswordAsync(string id)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null) return null;

        var newPassword = Guid.NewGuid().ToString();
        user.PasswordHash = passwordHasher.HashPassword(user, newPassword);
        user.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return newPassword;
    }

    public async Task<bool> ChangeGroupAsync(string userId, string groupId)
    {
        var user = await db.Users.FindAsync(userId);
        if (user == null) return false;

        var group = await db.UserGroups.FindAsync(groupId);
        if (group == null) return false;

        user.GroupId = groupId;
        user.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<User>> GetPendingApprovalsAsync()
    {
        return await db.Users
            .Include(u => u.Group)
            .Where(u => !u.IsApproved)
            .OrderBy(u => u.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }
}