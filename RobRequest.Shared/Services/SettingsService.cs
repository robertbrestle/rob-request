using Microsoft.EntityFrameworkCore;
using RobRequest.Shared.Data;
using RobRequest.Shared.Models;

namespace RobRequest.Shared.Services;

public class SettingsService(AppDbContext db, CurrentUserService currentUser)
{
    private UserSettings _settings = new();

    public event Action? OnSettingsChanged;

    public UserSettings Settings => _settings;

    public virtual async Task<UserSettings> GetSettingsAsync()
    {
        var userId = currentUser.UserId;
        if (string.IsNullOrEmpty(userId) || !currentUser.IsAuthenticated)
            return _settings;

        // if user doesn't exist, return _settings
        // TODO: refactor for better authentication/user status detection
        var user = await db.Users.FindAsync(userId);
        if (user is null)
            return _settings;

        var settings = await db.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId);
        if (settings is null)
        {
            settings = new UserSettings { UserId = userId };
            db.UserSettings.Add(settings);
            await db.SaveChangesAsync();
        }

        _settings = settings;
        return _settings;
    }

    public async Task UpdateSettingsAsync(UserSettings settings)
    {
        var userId = currentUser.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        var existing = await db.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId);
        if (existing is null)
        {
            settings.UserId = userId;
            db.UserSettings.Add(settings);
        }
        else
        {
            existing.DarkMode = settings.DarkMode;
            existing.DefaultTimeoutSeconds = settings.DefaultTimeoutSeconds;
            existing.MaxHistoryItems = settings.MaxHistoryItems;
            existing.AutoFormatJson = settings.AutoFormatJson;
            existing.ShowLineNumbers = settings.ShowLineNumbers;
            existing.FollowRedirects = settings.FollowRedirects;
            existing.ValidateSslCertificates = settings.ValidateSslCertificates;
            existing.ActiveEnvironmentId = settings.ActiveEnvironmentId;
            settings = existing;
        }

        await db.SaveChangesAsync();
        _settings = settings;
        OnSettingsChanged?.Invoke();
    }

    public async Task ToggleDarkModeAsync()
    {
        var settings = await GetSettingsAsync();
        settings.DarkMode = !settings.DarkMode;
        await db.SaveChangesAsync();
        OnSettingsChanged?.Invoke();
    }
}