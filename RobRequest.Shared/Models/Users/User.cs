using System.ComponentModel.DataAnnotations;

namespace RobRequest.Shared.Models.Users;

public class User
{
    [Key, StringLength(36)] public string Id { get; set; } = Guid.NewGuid().ToString();
    [StringLength(50)] public string Username { get; set; } = string.Empty;
    [StringLength(100)] public string PasswordHash { get; set; } = string.Empty;
    [StringLength(36)] public string GroupId { get; set; } = string.Empty;
    public UserGroup? Group { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public DateTime? LastLogin { get; set; }
}