using System.ComponentModel.DataAnnotations;

namespace RobRequest.Shared.Models;

public class CollectionModel
{
    [Key, StringLength(36)]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
    [StringLength(255)]
    public string? Description { get; set; }
    [StringLength(36)]
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    public string? ParentId { get; set; }
    public int SortOrder { get; set; }
    public CollectionModel? Parent { get; set; }
    public List<CollectionModel> Children { get; set; } = new();
    public List<CollectionRequestModel> Requests { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
