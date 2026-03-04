namespace RobRequest.Shared.Models;

public class UserSettings
{
    public bool DarkMode { get; set; } = true;
    public int DefaultTimeoutSeconds { get; set; } = 30;
    public int MaxHistoryItems { get; set; } = 1000;
    public bool AutoFormatJson { get; set; } = true;
    public bool FollowRedirects { get; set; } = true;
    public bool ValidateSslCertificates { get; set; } = true;
    public string? ActiveEnvironmentId { get; set; }
}
