namespace RobRequest.Shared.Models;

public class HttpRequestModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Method { get; set; } = "GET";
    public string Url { get; set; } = string.Empty;
    public List<HeaderItem> Headers { get; set; } = new();
    public List<QueryParamItem> QueryParams { get; set; } = new();
    public List<FormDataItem> FormData { get; set; } = new();
    public string BodyType { get; set; } = "none";
    public string Body { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/json";
    public AuthSettings Auth { get; set; } = new() { AuthType = AuthType.Inherit };
    public int TimeoutSeconds { get; set; } = 30;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string GetFullUrl()
    {
        if (string.IsNullOrWhiteSpace(Url))
            return string.Empty;

        var enabledParams = QueryParams.Where(p => p.Enabled && !string.IsNullOrWhiteSpace(p.Key)).ToList();

        // Add API key as query param if configured
        if (Auth.AuthType == AuthType.ApiKey && Auth.ApiKeyLocation == ApiKeyLocation.QueryParam &&
            !string.IsNullOrWhiteSpace(Auth.ApiKeyName) && !string.IsNullOrWhiteSpace(Auth.ApiKeyValue))
        {
            enabledParams.Add(new QueryParamItem
            {
                Key = Auth.ApiKeyName,
                Value = Auth.ApiKeyValue,
                Enabled = true
            });
        }

        if (enabledParams.Count == 0)
            return Url;

        var separator = Url.Contains('?') ? "&" : "?";
        var queryString = string.Join("&", enabledParams.Select(p =>
            $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

        return $"{Url}{separator}{queryString}";
    }

    public HttpRequestModel Clone(bool newId = true)
    {
        return new HttpRequestModel
        {
            Id = newId ? Guid.NewGuid().ToString() : Id,
            Method = Method,
            Url = Url,
            Headers =
                Headers.Select(h => new HeaderItem { Key = h.Key, Value = h.Value, Enabled = h.Enabled }).ToList(),
            QueryParams = QueryParams
                .Select(q => new QueryParamItem { Key = q.Key, Value = q.Value, Enabled = q.Enabled }).ToList(),
            FormData = FormData.Select(f => new FormDataItem
            {
                Key = f.Key,
                Value = f.Value,
                Enabled = f.Enabled,
                IsFile = f.IsFile,
                FileName = f.FileName,
                ContentType = f.ContentType
            }).ToList(),
            BodyType = BodyType,
            Body = Body,
            ContentType = ContentType,
            Auth = Auth.Clone(),
            TimeoutSeconds = TimeoutSeconds,
            CreatedAt = CreatedAt
        };
    }
}