using Microsoft.EntityFrameworkCore;
using RobRequest.Shared.Data;
using RobRequest.Shared.Models;

namespace RobRequest.Shared.Services;

public class SettingsService
{
    private readonly AppDbContext _db;
    private UserSettings _settings = new();

    public event Action? OnSettingsChanged;

    public SettingsService(AppDbContext db)
    {
        _db = db;
    }

    public UserSettings Settings => _settings;

    public async Task<UserSettings> GetSettingsAsync()
    {
        var settings = await _db.UserSettings.FindAsync("default");
        if (settings is null)
        {
            settings = new UserSettings();
            _db.UserSettings.Add(settings);
            await _db.SaveChangesAsync();
        }

        _settings = settings;
        return _settings;
    }

    public async Task UpdateSettingsAsync(UserSettings settings)
    {
        settings.Id = "default";
        var existing = await _db.UserSettings.FindAsync("default");
        if (existing is null)
        {
            _db.UserSettings.Add(settings);
        }
        else
        {
            _db.Entry(existing).CurrentValues.SetValues(settings);
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
