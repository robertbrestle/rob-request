using System.ComponentModel.DataAnnotations;

namespace RobRequest.Shared.Models.Collections;

public class Collection
{
    [Key, StringLength(36)] public string Id { get; set; } = Guid.NewGuid().ToString();
    [StringLength(50)] public string Name { get; set; } = string.Empty;
    [StringLength(255)] public string? Description { get; set; }
    [StringLength(36)] public string UserId { get; set; } = string.Empty;
    public Users.User? User { get; set; }
    [StringLength(36)] public string? ParentId { get; set; }
    public int SortOrder { get; set; }
    public Collection? Parent { get; set; }
    public List<Collection> Children { get; set; } = new();
    public List<CollectionRequest> Requests { get; set; } = new();
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}