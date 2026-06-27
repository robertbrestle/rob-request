using System.ComponentModel.DataAnnotations;

namespace RobRequest.Shared.Models;

public class UserSettings
{
    [Key, StringLength(36)]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [StringLength(36)]
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    public bool DarkMode { get; set; } = true;
    public int DefaultTimeoutSeconds { get; set; } = 30;
    public int MaxHistoryItems { get; set; } = 1000;
    public bool AutoFormatJson { get; set; } = true;
    public bool ShowLineNumbers { get; set; }
    public bool FollowRedirects { get; set; } = true;
    public bool ValidateSslCertificates { get; set; } = true;
    [StringLength(36)]
    public string? ActiveEnvironmentId { get; set; }
}