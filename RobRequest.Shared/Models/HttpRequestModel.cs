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
    public OAuth2GrantType OAuth2GrantType { get; set; } = OAuth2GrantType.ClientCredentials;
    public string OAuth2TokenUrl { get; set; } = string.Empty;
    public string OAuth2ClientId { get; set; } = string.Empty;
    public string OAuth2ClientSecret { get; set; } = string.Empty;
    public string OAuth2Scope { get; set; } = string.Empty;
    public DateTime? OAuth2TokenExpiresAt { get; set; }
    public bool OAuth2AutoRefresh { get; set; }
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

    public HttpRequestModel Clone(bool newId = true)
    {
        return new HttpRequestModel
        {
            Id = newId ? Guid.NewGuid().ToString() : Id,
            Method = Method,
            Url = Url,
            Headers = Headers.Select(h => new HeaderItem { Key = h.Key, Value = h.Value, Enabled = h.Enabled }).ToList(),
            QueryParams = QueryParams.Select(q => new QueryParamItem { Key = q.Key, Value = q.Value, Enabled = q.Enabled }).ToList(),
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
            AuthType = AuthType,
            AuthToken = AuthToken,
            AuthUsername = AuthUsername,
            AuthPassword = AuthPassword,
            ApiKeyName = ApiKeyName,
            ApiKeyValue = ApiKeyValue,
            ApiKeyLocation = ApiKeyLocation,
            OAuth2GrantType = OAuth2GrantType,
            OAuth2TokenUrl = OAuth2TokenUrl,
            OAuth2ClientId = OAuth2ClientId,
            OAuth2ClientSecret = OAuth2ClientSecret,
            OAuth2Scope = OAuth2Scope,
            OAuth2TokenExpiresAt = OAuth2TokenExpiresAt,
            OAuth2AutoRefresh = OAuth2AutoRefresh,
            TimeoutSeconds = TimeoutSeconds,
            CreatedAt = CreatedAt
        };
    }
}

public enum AuthType
{
    None,
    Bearer,
    Basic,
    ApiKey,
    OAuth2
}

public enum OAuth2GrantType
{
    ClientCredentials
}

public enum ApiKeyLocation
{
    Header,
    QueryParam
}
