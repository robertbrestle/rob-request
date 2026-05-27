using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using RobRequest.Shared.Data;
using RobRequest.Shared.Models;

namespace RobRequest.Shared.Services;

public partial class EnvironmentService
{
    private readonly AppDbContext _db;
    private readonly SettingsService _settingsService;
    private readonly CurrentUserService _currentUser;
    private string? _activeEnvironmentId;
    private bool _initialized;

    public event Action? OnEnvironmentChanged;

    public EnvironmentService(AppDbContext db, SettingsService settingsService, CurrentUserService currentUser)
    {
        _db = db;
        _settingsService = settingsService;
        _currentUser = currentUser;
    }

    public string? ActiveEnvironmentId
    {
        get => _activeEnvironmentId;
        set
        {
            _activeEnvironmentId = value;
            _ = PersistActiveEnvironmentIdAsync(value);
            OnEnvironmentChanged?.Invoke();
        }
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;

        var settings = await _settingsService.GetSettingsAsync();
        _activeEnvironmentId = settings.ActiveEnvironmentId;
    }

    private async Task PersistActiveEnvironmentIdAsync(string? id)
    {
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            settings.ActiveEnvironmentId = id;
            await _settingsService.UpdateSettingsAsync(settings);
        }
        catch
        {
            // Best-effort persistence; don't block UI
        }
    }

    public async Task<List<EnvironmentModel>> GetAllEnvironmentsAsync()
    {
        return await _db.Environments
            .Where(e => e.UserId == _currentUser.UserId)
            .OrderBy(e => e.SortOrder)
            .ThenBy(e => e.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<EnvironmentModel?> GetEnvironmentAsync(string id)
    {
        return await _db.Environments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<EnvironmentModel?> GetActiveEnvironmentAsync()
    {
        if (_activeEnvironmentId == null) return null;
        return await GetEnvironmentAsync(_activeEnvironmentId);
    }

    public async Task<EnvironmentModel> CreateEnvironmentAsync(string name, string? description = null)
    {
        var maxSort = await _db.Environments
            .Where(e => e.UserId == _currentUser.UserId)
            .MaxAsync(e => (int?)e.SortOrder) ?? -1;

        var environment = new EnvironmentModel
        {
            Name = name,
            Description = description,
            UserId = _currentUser.UserId ?? string.Empty,
            SortOrder = maxSort + 1,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _db.Environments.Add(environment);
        await _db.SaveChangesAsync();
        OnEnvironmentChanged?.Invoke();
        return environment;
    }

    public async Task UpdateEnvironmentAsync(EnvironmentModel environment)
    {
        var existing = await _db.Environments.FindAsync(environment.Id);
        if (existing == null) return;

        existing.Name = environment.Name;
        existing.Description = environment.Description;
        existing.SortOrder = environment.SortOrder;
        existing.Variables = environment.Variables.Select(v => new EnvironmentVariable
        {
            Key = v.Key,
            Value = v.Value,
            IsSecret = v.IsSecret,
            Enabled = v.Enabled
        }).ToList();
        
        existing.Auth.CopyFrom(environment.Auth);
        existing.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();
        OnEnvironmentChanged?.Invoke();
    }

    public async Task DeleteEnvironmentAsync(string id)
    {
        var environment = await _db.Environments.FindAsync(id);
        if (environment == null) return;

        _db.Environments.Remove(environment);
        await _db.SaveChangesAsync();

        if (_activeEnvironmentId == id)
        {
            _activeEnvironmentId = (await _db.Environments
                .Where(e => e.UserId == _currentUser.UserId)
                .OrderBy(e => e.SortOrder)
                .FirstOrDefaultAsync())?.Id;
        }

        OnEnvironmentChanged?.Invoke();
    }

    public async Task<List<EnvironmentModel>> SearchEnvironmentsAsync(string query)
    {
        query = query.ToLower();
        return await _db.Environments
            .Where(e => e.UserId == _currentUser.UserId)
            .Where(e => e.Name.ToLower().Contains(query) ||
                        (e.Description ?? "").ToLower().Contains(query))
            .OrderBy(e => e.SortOrder)
            .ThenBy(e => e.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task SetVariableAsync(string environmentId, string key, string value)
    {
        var env = await _db.Environments.FindAsync(environmentId);
        if (env == null) return;

        var variable = env.Variables.FirstOrDefault(v => v.Key == key);
        if (variable != null)
        {
            variable.Value = value;
        }
        else
        {
            env.Variables.Add(new EnvironmentVariable { Key = key, Value = value });
        }

        env.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        OnEnvironmentChanged?.Invoke();
    }

    public async Task<string> SubstituteVariablesAsync(string input)
    {
        if (string.IsNullOrEmpty(input) || _activeEnvironmentId == null)
            return input;

        var env = await _db.Environments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == _activeEnvironmentId);
        if (env == null)
            return input;

        var result = VariablePattern().Replace(input, match =>
        {
            var varName = match.Groups[1].Value;
            var variable = env.Variables.FirstOrDefault(v => v.Key == varName && v.Enabled);
            return variable?.Value ?? match.Value;
        });

        return result;
    }

    public async Task ClearAllEnvironmentsAsync()
    {
        await _db.Environments.Where(e => e.UserId == _currentUser.UserId).ExecuteDeleteAsync();
        _activeEnvironmentId = null;
        await PersistActiveEnvironmentIdAsync(null);
        OnEnvironmentChanged?.Invoke();
    }

    public static bool ContainsVariables(string? input)
    {
        return !string.IsNullOrEmpty(input) && VariablePattern().IsMatch(input);
    }

    [GeneratedRegex(@"\{\{([\w\-]+)\}\}")]
    private static partial Regex VariablePattern();
}
