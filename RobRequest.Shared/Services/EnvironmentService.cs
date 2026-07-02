using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using RobRequest.Shared.Data;
using RobRequest.Shared.Models;
using RobRequest.Shared.Models.Environments;
using Environment = RobRequest.Shared.Models.Environments.Environment;

namespace RobRequest.Shared.Services;

public partial class EnvironmentService(
    AppDbContext db,
    SettingsService settingsService,
    CurrentUserService currentUser)
{
    private string? _activeEnvironmentId;
    private bool _initialized;

    public event Action? OnEnvironmentChanged;

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

        var settings = await settingsService.GetSettingsAsync();
        _activeEnvironmentId = settings.ActiveEnvironmentId;
    }

    private async Task PersistActiveEnvironmentIdAsync(string? id)
    {
        try
        {
            var settings = await settingsService.GetSettingsAsync();
            settings.ActiveEnvironmentId = id;
            await settingsService.UpdateSettingsAsync(settings);
        }
        catch
        {
            // Best-effort persistence; don't block UI
        }
    }

    public async Task<List<Environment>> GetAllEnvironmentsAsync()
    {
        return await db.Environments
            .Where(e => e.UserId == currentUser.UserId)
            .OrderBy(e => e.SortOrder)
            .ThenBy(e => e.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Environment?> GetEnvironmentAsync(string id)
    {
        return await db.Environments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Environment?> GetActiveEnvironmentAsync()
    {
        if (_activeEnvironmentId == null) return null;
        return await GetEnvironmentAsync(_activeEnvironmentId);
    }

    public async Task<Environment> CreateEnvironmentAsync(string name, string? description = null)
    {
        var maxSort = await db.Environments
            .Where(e => e.UserId == currentUser.UserId)
            .MaxAsync(e => (int?)e.SortOrder) ?? -1;

        var environment = new Environment
        {
            Name = name,
            Description = description,
            UserId = currentUser.UserId ?? string.Empty,
            SortOrder = maxSort + 1,
            UpdatedAt = DateTime.Now
        };

        db.Environments.Add(environment);
        await db.SaveChangesAsync();
        OnEnvironmentChanged?.Invoke();
        return environment;
    }

    public async Task UpdateEnvironmentAsync(Environment environment)
    {
        var existing = await db.Environments.FindAsync(environment.Id);
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

        await db.SaveChangesAsync();
        OnEnvironmentChanged?.Invoke();
    }

    public async Task DeleteEnvironmentAsync(string id)
    {
        var environment = await db.Environments.FindAsync(id);
        if (environment == null) return;

        db.Environments.Remove(environment);
        await db.SaveChangesAsync();

        if (_activeEnvironmentId == id)
        {
            _activeEnvironmentId = (await db.Environments
                .Where(e => e.UserId == currentUser.UserId)
                .OrderBy(e => e.SortOrder)
                .FirstOrDefaultAsync())?.Id;
        }

        OnEnvironmentChanged?.Invoke();
    }

    public async Task<List<Environment>> SearchEnvironmentsAsync(string query)
    {
        query = query.ToLower();
        return await db.Environments
            .Where(e => e.UserId == currentUser.UserId)
            .Where(e => e.Name.ToLower().Contains(query) ||
                        (e.Description ?? "").ToLower().Contains(query))
            .OrderBy(e => e.SortOrder)
            .ThenBy(e => e.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task SetVariableAsync(string environmentId, string key, string value)
    {
        var env = await db.Environments.FindAsync(environmentId);
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
        await db.SaveChangesAsync();
        OnEnvironmentChanged?.Invoke();
    }

    public async Task<string> SubstituteVariablesAsync(string input)
    {
        if (string.IsNullOrEmpty(input) || _activeEnvironmentId == null)
            return input;

        var env = await db.Environments
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
        await db.Environments.Where(e => e.UserId == currentUser.UserId).ExecuteDeleteAsync();
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