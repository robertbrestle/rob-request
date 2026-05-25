namespace RobRequest.Tests.Unit.Models;

public class HttpRequestModelTests
{
    [Fact]
    public void GetFullUrl_ReturnsUrlWhenNoParams()
    {
        var model = new HttpRequestModel { Url = "https://example.com" };

        model.GetFullUrl().Should().Be("https://example.com");
    }

    [Fact]
    public void GetFullUrl_AppendsQueryParams()
    {
        var model = new HttpRequestModel
        {
            Url = "https://example.com",
            QueryParams = new List<QueryParamItem>
            {
                new() { Key = "page", Value = "1", Enabled = true },
                new() { Key = "limit", Value = "10", Enabled = true }
            }
        };

        var url = model.GetFullUrl();

        url.Should().Be("https://example.com?page=1&limit=10");
    }

    [Fact]
    public void GetFullUrl_SkipsDisabledParams()
    {
        var model = new HttpRequestModel
        {
            Url = "https://example.com",
            QueryParams = new List<QueryParamItem>
            {
                new() { Key = "active", Value = "true", Enabled = true },
                new() { Key = "disabled", Value = "skip", Enabled = false }
            }
        };

        var url = model.GetFullUrl();

        url.Should().Be("https://example.com?active=true");
        url.Should().NotContain("disabled");
    }

    [Fact]
    public void GetFullUrl_ReturnsEmptyForEmptyUrl()
    {
        var model = new HttpRequestModel { Url = "" };

        model.GetFullUrl().Should().BeEmpty();
    }

    [Fact]
    public void GetFullUrl_AddsApiKeyAsQueryParam()
    {
        var model = new HttpRequestModel
        {
            Url = "https://example.com",
            Auth =
            {
                AuthType = AuthType.ApiKey,
                ApiKeyLocation = ApiKeyLocation.QueryParam,
                ApiKeyName = "api_key",
                ApiKeyValue = "secret123"
            }
        };

        var url = model.GetFullUrl();

        url.Should().Contain("api_key=secret123");
    }

    [Fact]
    public void Defaults_AreCorrect()
    {
        var model = new HttpRequestModel();

        model.Method.Should().Be("GET");
        model.Url.Should().BeEmpty();
        model.BodyType.Should().Be("none");
        model.ContentType.Should().Be("application/json");
        model.Auth.AuthType.Should().Be(AuthType.Inherit);
        model.TimeoutSeconds.Should().Be(30);
        model.Headers.Should().BeEmpty();
        model.QueryParams.Should().BeEmpty();
    }

    [Fact]
    public void Clone_ShouldCopyAllFields()
    {
        // Arrange
        var model = new HttpRequestModel
        {
            Method = "POST",
            Url = "https://example.com",
            Headers = new List<HeaderItem> { new() { Key = "K1", Value = "V1" } },
            QueryParams = new List<QueryParamItem> { new() { Key = "Q1", Value = "V1" } },
            FormData = new List<FormDataItem> { new() { Key = "F1", Value = "V1" } },
            BodyType = "json",
            Body = "{}",
            ContentType = "application/json",
            Auth =
            {
                AuthType = AuthType.OAuth2,
                AuthToken = "token",
                OAuth2GrantType = OAuth2GrantType.ClientCredentials,
                OAuth2TokenUrl = "url",
                OAuth2ClientId = "id",
                OAuth2ClientSecret = "secret",
                OAuth2Scope = "scope",
                OAuth2TokenExpiresAt = DateTime.Now.AddHours(1),
                OAuth2AutoRefresh = true
            },
            TimeoutSeconds = 60
        };

        // Act
        var clone = model.Clone(newId: false);

        // Assert
        clone.Should().BeEquivalentTo(model);
        clone.Id.Should().Be(model.Id);
        
        // Deep copy check
        clone.Headers.Should().NotBeSameAs(model.Headers);
        clone.Headers[0].Should().NotBeSameAs(model.Headers[0]);
    }

    [Fact]
    public void Clone_WithNewId_ShouldGenerateNewGuid()
    {
        // Arrange
        var model = new HttpRequestModel();

        // Act
        var clone = model.Clone(newId: true);

        // Assert
        clone.Id.Should().NotBe(model.Id);
        Guid.TryParse(clone.Id, out _).Should().BeTrue();
    }
}
