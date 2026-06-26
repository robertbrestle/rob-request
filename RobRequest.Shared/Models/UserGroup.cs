using System.ComponentModel.DataAnnotations;

namespace RobRequest.Shared.Models;

public class UserGroup
{
    [Key, StringLength(36)]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [StringLength(36)]
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}