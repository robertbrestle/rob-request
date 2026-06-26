using System.ComponentModel.DataAnnotations;

namespace RobRequest.Shared.Models;

public class CollectionRequestModel
{
    [Key, StringLength(36)]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [StringLength(36)]
    public string CollectionId { get; set; } = string.Empty;
    public CollectionModel? Collection { get; set; }
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public HttpRequestModel Request { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}