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
            AuthType = AuthType.ApiKey,
            ApiKeyLocation = ApiKeyLocation.QueryParam,
            ApiKeyName = "api_key",
            ApiKeyValue = "secret123"
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
        model.AuthType.Should().Be(AuthType.None);
        model.TimeoutSeconds.Should().Be(30);
        model.Headers.Should().BeEmpty();
        model.QueryParams.Should().BeEmpty();
    }
}
