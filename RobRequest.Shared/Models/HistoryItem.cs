using System.ComponentModel.DataAnnotations;

namespace RobRequest.Shared.Models;

public class HistoryItem
{
    [Key]
    public string Id { get; set; } = Guid.CreateVersion7().ToString();
    public string Method { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long ResponseTimeMs { get; set; }
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public HttpRequestModel? Request { get; set; }
    public HttpResponseModel? Response { get; set; }

    public string StatusColor => StatusCode switch
    {
        >= 200 and < 300 => "success",
        >= 300 and < 400 => "info",
        >= 400 and < 500 => "warning",
        >= 500 => "error",
        _ => "default"
    };
}
