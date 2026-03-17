namespace RobRequest.Shared.Services;

public class CurrentUserService
{
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public string? GroupName { get; set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(UserId);
    public bool IsAdmin => string.Equals(GroupName, "admin", StringComparison.OrdinalIgnoreCase);
}
