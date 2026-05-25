using Microsoft.EntityFrameworkCore;
using RobRequest.Shared.Data;
using RobRequest.Shared.Models;

namespace RobRequest.Shared.Services;

public class SettingsService
{
    private readonly AppDbContext _db;
    private readonly CurrentUserService _currentUser;
    private UserSettings _settings = new();

    public event Action? OnSettingsChanged;

    public SettingsService(AppDbContext db, CurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public UserSettings Settings => _settings;

    public async Task<UserSettings> GetSettingsAsync()
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            return _settings;
        }

        var settings = await _db.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId);
        if (settings is null)
        {
            settings = new UserSettings { UserId = userId };
            _db.UserSettings.Add(settings);
            await _db.SaveChangesAsync();
        }

        _settings = settings;
        return _settings;
    }

    public async Task UpdateSettingsAsync(UserSettings settings)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        var existing = await _db.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId);
        if (existing is null)
        {
            settings.UserId = userId;
            _db.UserSettings.Add(settings);
        }
        else
        {
            existing.DarkMode = settings.DarkMode;
            existing.DefaultTimeoutSeconds = settings.DefaultTimeoutSeconds;
            existing.MaxHistoryItems = settings.MaxHistoryItems;
            existing.AutoFormatJson = settings.AutoFormatJson;
            existing.FollowRedirects = settings.FollowRedirects;
            existing.ValidateSslCertificates = settings.ValidateSslCertificates;
            existing.ActiveEnvironmentId = settings.ActiveEnvironmentId;
            settings = existing;
        }

        await _db.SaveChangesAsync();
        _settings = settings;
        OnSettingsChanged?.Invoke();
    }

    public async Task ToggleDarkModeAsync()
    {
        var settings = await GetSettingsAsync();
        settings.DarkMode = !settings.DarkMode;
        await _db.SaveChangesAsync();
        OnSettingsChanged?.Invoke();
    }
}