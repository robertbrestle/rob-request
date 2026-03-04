namespace RobRequest.Shared.Models;

public class HttpResponseModel
{
    public int StatusCode { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public List<HeaderItem> Headers { get; set; } = new();
    public string ContentType { get; set; } = string.Empty;
    public long ResponseTimeMs { get; set; }
    public long ResponseSizeBytes { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public string? ErrorMessage { get; set; }
    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;

    public string StatusColor => StatusCode switch
    {
        >= 200 and < 300 => "success",
        >= 300 and < 400 => "info",
        >= 400 and < 500 => "warning",
        >= 500 => "error",
        _ => "default"
    };

    public string FormattedSize
    {
        get
        {
            if (ResponseSizeBytes < 1024)
                return $"{ResponseSizeBytes} B";
            if (ResponseSizeBytes < 1024 * 1024)
                return $"{ResponseSizeBytes / 1024.0:F1} KB";
            return $"{ResponseSizeBytes / (1024.0 * 1024.0):F1} MB";
        }
    }
}
