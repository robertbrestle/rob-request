using RobRequest.Shared.Models.Requests;

namespace RobRequest.Shared.Models.Export;

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