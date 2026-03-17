using System.ComponentModel.DataAnnotations;

namespace RobRequest.Shared.Models;

public class CollectionModel
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
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
