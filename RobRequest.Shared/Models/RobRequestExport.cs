namespace RobRequest.Shared.Models;

public class RobRequestExport
{
    public string FormatVersion { get; set; } = "1";
    public string AppVersion { get; set; } = string.Empty;
    public DateTime ExportedAt { get; set; }
    public List<ExportedCollection>? Collections { get; set; }
    public List<ExportedEnvironment>? Environments { get; set; }
    public List<ExportedHistoryItem>? History { get; set; }
}

public class ExportedCollection
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ParentId { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ExportedCollectionRequest> Requests { get; set; } = new();
}

public class ExportedCollectionRequest
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public HttpRequestModel Request { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ExportedEnvironment
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public List<EnvironmentVariable> Variables { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ExportedHistoryItem
{
    public string Method { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long ResponseTimeMs { get; set; }
    public DateTime Timestamp { get; set; }
    public HttpRequestModel? Request { get; set; }
    public HttpResponseModel? Response { get; set; }
}
