namespace RobRequest.Shared.Models;

public class FormDataItem
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public bool IsFile { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
}
