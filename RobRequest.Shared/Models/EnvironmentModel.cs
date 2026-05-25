using System.ComponentModel.DataAnnotations;

namespace RobRequest.Shared.Models;

public class EnvironmentModel
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public List<EnvironmentVariable> Variables { get; set; } = new();
    public AuthSettings Auth { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public EnvironmentModel Clone()
    {
        return new EnvironmentModel
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

public class EnvironmentVariable
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsSecret { get; set; }
    public bool Enabled { get; set; } = true;
}
