namespace RobRequest.Shared.Models;

public class CollectionModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ParentId { get; set; }
    public List<CollectionModel> Children { get; set; } = new();
    public List<HttpRequestModel> Requests { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
