using System.ComponentModel.DataAnnotations;
using RobRequest.Shared.Models.Requests;

namespace RobRequest.Shared.Models.Collections;

public class CollectionRequest
{
    [Key, StringLength(36)] public string Id { get; set; } = Guid.NewGuid().ToString();
    [StringLength(36)] public string CollectionId { get; set; } = string.Empty;
    public Collection? Collection { get; set; }
    [StringLength(50)] public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public HttpRequestModel Request { get; set; } = new();
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}