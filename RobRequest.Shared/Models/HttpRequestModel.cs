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
    public AuthType AuthType { get; set; } = AuthType.None;
    public string AuthToken { get; set; } = string.Empty;
    public string AuthUsername { get; set; } = string.Empty;
    public string AuthPassword { get; set; } = string.Empty;
    public string ApiKeyName { get; set; } = string.Empty;
    public string ApiKeyValue { get; set; } = string.Empty;
    public ApiKeyLocation ApiKeyLocation { get; set; } = ApiKeyLocation.Header;
    public int TimeoutSeconds { get; set; } = 30;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string GetFullUrl()
    {
        if (string.IsNullOrWhiteSpace(Url))
            return string.Empty;

        var enabledParams = QueryParams.Where(p => p.Enabled && !string.IsNullOrWhiteSpace(p.Key)).ToList();

        // Add API key as query param if configured
        if (AuthType == AuthType.ApiKey && ApiKeyLocation == ApiKeyLocation.QueryParam &&
            !string.IsNullOrWhiteSpace(ApiKeyName) && !string.IsNullOrWhiteSpace(ApiKeyValue))
        {
            enabledParams.Add(new QueryParamItem
            {
                Key = ApiKeyName,
                Value = ApiKeyValue,
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
}

public enum AuthType
{
    None,
    Bearer,
    Basic,
    ApiKey
}

public enum ApiKeyLocation
{
    Header,
    QueryParam
}
