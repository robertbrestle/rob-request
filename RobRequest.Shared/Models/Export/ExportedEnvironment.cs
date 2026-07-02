using RobRequest.Shared.Models.Auth;
using RobRequest.Shared.Models.Environments;

namespace RobRequest.Shared.Models.Export;

public class ExportedEnvironment
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public List<EnvironmentVariable> Variables { get; set; } = new();
    public AuthSettings Auth { get; set; } = new();
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; }
}