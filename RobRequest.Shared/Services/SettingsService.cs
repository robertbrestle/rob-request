using RobRequest.Shared.Models;

namespace RobRequest.Shared.Services;

public class SettingsService
{
    private UserSettings _settings = new();

    public event Action? OnSettingsChanged;

    public UserSettings Settings => _settings;

    public Task<UserSettings> GetSettingsAsync()
    {
        return Task.FromResult(_settings);
    }

    public Task UpdateSettingsAsync(UserSettings settings)
    {
        _settings = settings;
        OnSettingsChanged?.Invoke();
        return Task.CompletedTask;
    }

    public Task ToggleDarkModeAsync()
    {
        _settings.DarkMode = !_settings.DarkMode;
        OnSettingsChanged?.Invoke();
        return Task.CompletedTask;
    }
}
