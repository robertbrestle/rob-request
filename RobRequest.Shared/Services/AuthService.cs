using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RobRequest.Shared.Data;
using RobRequest.Shared.Models;

namespace RobRequest.Shared.Services;

public class AuthService(AppDbContext db, IPasswordHasher<User> passwordHasher)
{
    public async Task<(bool Success, string? Error)> RegisterAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return (false, "Username and password are required.");

        if (password.Length < 8)
            return (false, "Password must be at least 8 characters.");

        var exists = await db.Users.AnyAsync(u => u.Username == username);
        if (exists)
            return (false, "Username is already taken.");

        var userGroup = await db.UserGroups.FirstOrDefaultAsync(g => g.Name == "user");
        if (userGroup == null)
            return (false, "User group not found. Please contact an administrator.");

        var user = new User
        {
            Username = username,
            GroupId = userGroup.Id,
            IsEnabled = true,
            IsApproved = false
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

    public async Task<User?> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        var user = await db.Users
            .Include(u => u.Group)
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
            return null;

        if (!user.IsEnabled || !user.IsApproved)
            return null;

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
            return null;

        // Rehash if needed (e.g. algorithm upgrade)
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, password);
            await db.SaveChangesAsync();
        }

        user.LastLogin = DateTime.Now;
        await db.SaveChangesAsync();

        return user;
    }

    public async Task<bool> ChangePasswordAsync(string userId, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            return false;

        var user = await db.Users.FindAsync(userId);
        if (user == null)
            return false;

        user.PasswordHash = passwordHasher.HashPassword(user, newPassword);
        user.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return true;
    }
}