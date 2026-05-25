using System.ComponentModel.DataAnnotations;

namespace RobRequest.Shared.Models;

public enum AuthType
{
    None,
    Inherit,
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

public class AuthSettings
{
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

    public AuthSettings Clone()
    {
        return new AuthSettings
        {
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
            OAuth2AutoRefresh = OAuth2AutoRefresh
        };
    }

    public void CopyFrom(AuthSettings other)
    {
        AuthType = other.AuthType;
        AuthToken = other.AuthToken;
        AuthUsername = other.AuthUsername;
        AuthPassword = other.AuthPassword;
        ApiKeyName = other.ApiKeyName;
        ApiKeyValue = other.ApiKeyValue;
        ApiKeyLocation = other.ApiKeyLocation;
        OAuth2GrantType = other.OAuth2GrantType;
        OAuth2TokenUrl = other.OAuth2TokenUrl;
        OAuth2ClientId = other.OAuth2ClientId;
        OAuth2ClientSecret = other.OAuth2ClientSecret;
        OAuth2Scope = other.OAuth2Scope;
        OAuth2TokenExpiresAt = other.OAuth2TokenExpiresAt;
        OAuth2AutoRefresh = other.OAuth2AutoRefresh;
    }
}
