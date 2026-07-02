namespace RobRequest.Shared.Models.Requests;

public class FormDataItem : KeyValueEntry
{
    public bool IsFile { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
}