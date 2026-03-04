using System.Text.RegularExpressions;
using RobRequest.Shared.Models;

namespace RobRequest.Shared.Services;

public partial class EnvironmentService
{
    private readonly List<EnvironmentModel> _environments = new();
    private string? _activeEnvironmentId;

    public event Action? OnEnvironmentChanged;

    public EnvironmentService()
    {
        // Create a default environment
        var defaultEnv = new EnvironmentModel
        {
            Name = "Default",
            Variables = new List<EnvironmentVariable>()
        };
        _environments.Add(defaultEnv);
        _activeEnvironmentId = defaultEnv.Id;
    }

    public string? ActiveEnvironmentId
    {
        get => _activeEnvironmentId;
        set
        {
            _activeEnvironmentId = value;
            OnEnvironmentChanged?.Invoke();
        }
    }

    public Task<IReadOnlyList<EnvironmentModel>> GetEnvironmentsAsync()
    {
        return Task.FromResult<IReadOnlyList<EnvironmentModel>>(_environments.ToList());
    }

    public Task<EnvironmentModel?> GetActiveEnvironmentAsync()
    {
        var env = _environments.FirstOrDefault(e => e.Id == _activeEnvironmentId);
        return Task.FromResult(env);
    }

    public Task<EnvironmentModel?> GetEnvironmentAsync(string id)
    {
        var env = _environments.FirstOrDefault(e => e.Id == id);
        return Task.FromResult(env);
    }

    public Task AddEnvironmentAsync(EnvironmentModel environment)
    {
        _environments.Add(environment);
        OnEnvironmentChanged?.Invoke();
        return Task.CompletedTask;
    }

    public Task UpdateEnvironmentAsync(EnvironmentModel environment)
    {
        var index = _environments.FindIndex(e => e.Id == environment.Id);
        if (index >= 0)
        {
            environment.UpdatedAt = DateTime.UtcNow;
            _environments[index] = environment;
            OnEnvironmentChanged?.Invoke();
        }
        return Task.CompletedTask;
    }

    public Task DeleteEnvironmentAsync(string id)
    {
        _environments.RemoveAll(e => e.Id == id);
        if (_activeEnvironmentId == id)
            _activeEnvironmentId = _environments.FirstOrDefault()?.Id;
        OnEnvironmentChanged?.Invoke();
        return Task.CompletedTask;
    }

    public Task SetVariableAsync(string environmentId, string key, string value)
    {
        var env = _environments.FirstOrDefault(e => e.Id == environmentId);
        if (env == null) return Task.CompletedTask;

        var variable = env.Variables.FirstOrDefault(v => v.Key == key);
        if (variable != null)
        {
            variable.Value = value;
        }
        else
        {
            env.Variables.Add(new EnvironmentVariable { Key = key, Value = value });
        }

        env.UpdatedAt = DateTime.UtcNow;
        OnEnvironmentChanged?.Invoke();
        return Task.CompletedTask;
    }

    public Task<string> SubstituteVariablesAsync(string input)
    {
        if (string.IsNullOrEmpty(input) || _activeEnvironmentId == null)
            return Task.FromResult(input);

        var env = _environments.FirstOrDefault(e => e.Id == _activeEnvironmentId);
        if (env == null)
            return Task.FromResult(input);

        var result = VariablePattern().Replace(input, match =>
        {
            var varName = match.Groups[1].Value;
            var variable = env.Variables.FirstOrDefault(v => v.Key == varName && v.Enabled);
            return variable?.Value ?? match.Value;
        });

        return Task.FromResult(result);
    }

    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex VariablePattern();
}
