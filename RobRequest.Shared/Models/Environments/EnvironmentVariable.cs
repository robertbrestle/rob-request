using System.ComponentModel.DataAnnotations;

namespace RobRequest.Shared.Models.Environments;

public class EnvironmentVariable
{
    [StringLength(50)] public string Key { get; set; } = string.Empty;
    [StringLength(255)] public string Value { get; set; } = string.Empty;
    public bool IsSecret { get; set; }
    public bool Enabled { get; set; } = true;
}