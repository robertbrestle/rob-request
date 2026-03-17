using System.ComponentModel.DataAnnotations;

namespace RobRequest.Shared.Models;

public class User
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public UserGroup? Group { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
