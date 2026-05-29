namespace RobRequest.Shared.Services;

public class CurrentUserService
{
    private string? _userId;
    private string? _username;
    private string? _groupName;

    public string? UserId
    {
        get => _userId;
        set
        {
            if (_userId == value) return;
            _userId = value;
            OnUserChanged?.Invoke();
        }
    }

    public string? Username
    {
        get => _username;
        set
        {
            if (_username == value) return;
            _username = value;
            OnUserChanged?.Invoke();
        }
    }

    public string? GroupName
    {
        get => _groupName;
        set
        {
            if (_groupName == value) return;
            _groupName = value;
            OnUserChanged?.Invoke();
        }
    }

    public void SetUser(string? userId, string? username, string? groupName)
    {
        var changed = _userId != userId || _username != username || _groupName != groupName;
        _userId = userId;
        _username = username;
        _groupName = groupName;

        if (changed)
        {
            OnUserChanged?.Invoke();
        }
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(UserId);
    public bool IsAdmin => string.Equals(GroupName, "admin", StringComparison.OrdinalIgnoreCase);

    public event Action? OnUserChanged;
}
