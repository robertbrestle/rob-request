using System.Net;
using Moq.Protected;

namespace RobRequest.Tests.Unit.Services;

public class ApiServiceTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory = new();
    private readonly Mock<SettingsService> _mockSettingsService;

    public ApiServiceTests()
    {
        var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        var dbContext = new AppDbContext(dbContextOptions);
        var currentUser = new CurrentUserService(); // Not authenticated by default
        _mockSettingsService = new Mock<SettingsService>(dbContext, currentUser);
        _mockSettingsService.Setup(s => s.GetSettingsAsync()).ReturnsAsync(new UserSettings());
    }

    private ApiService CreateApiService(HttpClient httpClient)
    {
        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
        return new ApiService(_mockHttpClientFactory.Object, _mockSettingsService.Object);
    }

    [Fact]
    public async Task GetOAuth2TokenAsync_ShouldReturnToken_WhenSuccessful()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"access_token\":\"test-token\",\"expires_in\":3600}")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var apiService = CreateApiService(httpClient);
        var request = new HttpRequestModel
        {
            Auth =
            {
                OAuth2TokenUrl = "https://example.com/token",
                OAuth2ClientId = "client-id",
                OAuth2ClientSecret = "client-secret"
            }
        };

        // Act
        var result = await apiService.GetOAuth2TokenAsync(request);

        // Assert
        result.AccessToken.Should().Be("test-token");
        result.ExpiresIn.Should().Be(3600);
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri!.ToString() == "https://example.com/token"),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetOAuth2TokenAsync_ShouldThrowException_WhenTokenUrlIsMissing()
    {
        // Arrange
        var httpClient = new HttpClient();
        var apiService = CreateApiService(httpClient);
        var request = new HttpRequestModel { Auth = { OAuth2TokenUrl = "" } };

        // Act
        Func<Task> act = () => apiService.GetOAuth2TokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Token URL is required.");
    }

    [Fact]
    public async Task GetOAuth2TokenAsync_ShouldThrowException_WhenResponseFails()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Invalid client")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var apiService = CreateApiService(httpClient);
        var request = new HttpRequestModel
        {
            Auth =
            {
                OAuth2TokenUrl = "https://example.com/token",
                OAuth2ClientId = "id",
                OAuth2ClientSecret = "secret"
            }
        };

        // Act
        Func<Task> act = () => apiService.GetOAuth2TokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>().WithMessage("*Failed to get token*");
    }

    [Theory]
    [InlineData(true, AuthType.None, "Default")]
    [InlineData(false, AuthType.None, "NoSslValidation")]
    [InlineData(false, AuthType.Basic, "Default")]
    [InlineData(true, AuthType.Basic, "Default")]
    public async Task SendRequestAsync_ShouldUseCorrectHttpClient_BasedOnSettingsAndAuth(bool validateSsl, AuthType authType, string expectedClientName)
    {
        // Arrange
        _mockSettingsService.Setup(s => s.GetSettingsAsync()).ReturnsAsync(new UserSettings { ValidateSslCertificates = validateSsl });
        
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("") });

        var httpClient = new HttpClient(mockHandler.Object);
        _mockHttpClientFactory.Setup(f => f.CreateClient(expectedClientName)).Returns(httpClient);
        
        var apiService = new ApiService(_mockHttpClientFactory.Object, _mockSettingsService.Object);
        var request = new HttpRequestModel 
        { 
            Url = "https://example.com",
            Auth = { AuthType = authType } 
        };

        // Act
        await apiService.SendRequestAsync(request);

        // Assert
        _mockHttpClientFactory.Verify(f => f.CreateClient(expectedClientName), Times.Once);
    }
}
