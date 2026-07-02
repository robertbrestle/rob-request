namespace RobRequest.Shared.Models.Export;

public class Export
{
    public string FormatVersion { get; set; } = "1";
    public string AppVersion { get; set; } = string.Empty;
    public DateTime ExportedAt { get; set; }
    public List<ExportedCollection>? Collections { get; set; }
    public List<ExportedEnvironment>? Environments { get; set; }
    public List<ExportedHistoryItem>? History { get; set; }
}