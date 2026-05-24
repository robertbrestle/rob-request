using System.Net;
using Moq.Protected;

namespace RobRequest.Tests.Unit.Services;

public class ApiServiceTests
{
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
                Content = new StringContent("{\"access_token\":\"test-token\"}")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var apiService = new ApiService(httpClient);
        var request = new HttpRequestModel
        {
            OAuth2TokenUrl = "https://example.com/token",
            OAuth2ClientId = "client-id",
            OAuth2ClientSecret = "client-secret"
        };

        // Act
        var token = await apiService.GetOAuth2TokenAsync(request);

        // Assert
        token.Should().Be("test-token");
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
        var apiService = new ApiService(httpClient);
        var request = new HttpRequestModel { OAuth2TokenUrl = "" };

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
        var apiService = new ApiService(httpClient);
        var request = new HttpRequestModel
        {
            OAuth2TokenUrl = "https://example.com/token",
            OAuth2ClientId = "id",
            OAuth2ClientSecret = "secret"
        };

        // Act
        Func<Task> act = () => apiService.GetOAuth2TokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>().WithMessage("*Failed to get token*");
    }
}
