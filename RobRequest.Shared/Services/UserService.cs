using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RobRequest.Shared.Data;
using RobRequest.Shared.Models;

namespace RobRequest.Shared.Services;

public class UserService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(AppDbContext db, IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _db.Users
            .Include(u => u.Group)
            .OrderBy(u => u.Username)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<User?> GetUserAsync(string id)
    {
        return await _db.Users
            .Include(u => u.Group)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<List<UserGroup>> GetAllGroupsAsync()
    {
        return await _db.UserGroups
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

        var exists = await _db.Users.AnyAsync(u => u.Username == username);
        if (exists)
            return (false, "Username is already taken.");

        var group = await _db.UserGroups.FindAsync(groupId);
        if (group == null)
            return (false, "Invalid group.");

        var user = new User
        {
            Username = username,
            GroupId = groupId,
            IsEnabled = true,
            IsApproved = true // Admin-created users are pre-approved
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        _db.Users.Add(user);

        // Create default settings for the new user
        var settings = new UserSettings
        {
            UserId = user.Id
        };
        _db.UserSettings.Add(settings);

        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return false;

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleEnabledAsync(string id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return false;

        user.IsEnabled = !user.IsEnabled;
        user.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ApproveUserAsync(string id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return false;

        user.IsApproved = true;
        user.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<string?> ResetPasswordAsync(string id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return null;

        var newPassword = Guid.NewGuid().ToString();
        user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
        user.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return newPassword;
    }

    public async Task<bool> ChangeGroupAsync(string userId, string groupId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return false;

        var group = await _db.UserGroups.FindAsync(groupId);
        if (group == null) return false;

        user.GroupId = groupId;
        user.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<User>> GetPendingApprovalsAsync()
    {
        return await _db.Users
            .Include(u => u.Group)
            .Where(u => !u.IsApproved)
            .OrderBy(u => u.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }
}
