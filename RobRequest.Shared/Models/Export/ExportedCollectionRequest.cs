using RobRequest.Shared.Models.Requests;

namespace RobRequest.Shared.Models.Export;

public class ExportedCollectionRequest
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public HttpRequestModel Request { get; set; } = new();
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; }
}