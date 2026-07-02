using System.ComponentModel.DataAnnotations;
using RobRequest.Shared.Models.Auth;

namespace RobRequest.Shared.Models.Environments;

public class Environment
{
    [Key, StringLength(36)] public string Id { get; set; } = Guid.NewGuid().ToString();
    [StringLength(36)] public string UserId { get; set; } = string.Empty;
    public Users.User? User { get; set; }
    [StringLength(50)] public string Name { get; set; } = string.Empty;
    [StringLength(255)] public string? Description { get; set; }
    public int SortOrder { get; set; }
    public List<EnvironmentVariable> Variables { get; set; } = new();
    public AuthSettings Auth { get; set; } = new();
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public Environment Clone()
    {
        return new Environment
        {
            Id = Id,
            UserId = UserId,
            Name = Name,
            Description = Description,
            SortOrder = SortOrder,
            Variables = Variables.Select(v => new EnvironmentVariable
            {
                Key = v.Key,
                Value = v.Value,
                IsSecret = v.IsSecret,
                Enabled = v.Enabled
            }).ToList(),
            Auth = Auth.Clone(),
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };
    }
}