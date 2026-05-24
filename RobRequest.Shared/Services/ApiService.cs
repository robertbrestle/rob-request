using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using RobRequest.Shared.Models;

namespace RobRequest.Shared.Services;

public class ApiService(HttpClient httpClient)
{
    public async Task<HttpResponseModel> SendRequestAsync(HttpRequestModel request, CancellationToken ct = default)
    {
        var response = new HttpResponseModel();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var fullUrl = request.GetFullUrl();
            if (string.IsNullOrWhiteSpace(fullUrl))
            {
                response.ErrorMessage = "URL is required.";
                return response;
            }

            if (!Uri.TryCreate(fullUrl, UriKind.Absolute, out var uri))
            {
                response.ErrorMessage = "Invalid URL format.";
                return response;
            }

            using var httpRequest = new HttpRequestMessage(new HttpMethod(request.Method), uri);

            ApplyHeaders(httpRequest, request);
            ApplyAuthentication(httpRequest, request);
            ApplyBody(httpRequest, request);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(request.TimeoutSeconds));

            using var httpResponse = await httpClient.SendAsync(httpRequest, cts.Token);

            stopwatch.Stop();

            response.StatusCode = (int)httpResponse.StatusCode;
            response.StatusText = httpResponse.ReasonPhrase ?? httpResponse.StatusCode.ToString();
            response.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            response.ReceivedAt = DateTime.Now;

            // Read response headers
            foreach (var header in httpResponse.Headers)
            {
                response.Headers.Add(new HeaderItem
                {
                    Key = header.Key,
                    Value = string.Join(", ", header.Value)
                });
            }

            foreach (var header in httpResponse.Content.Headers)
            {
                response.Headers.Add(new HeaderItem
                {
                    Key = header.Key,
                    Value = string.Join(", ", header.Value)
                });
            }

            // Read content type
            response.ContentType = httpResponse.Content.Headers.ContentType?.MediaType ?? string.Empty;

            // Read body
            var bodyBytes = await httpResponse.Content.ReadAsByteArrayAsync(cts.Token);
            response.ResponseSizeBytes = bodyBytes.Length;
            response.Body = Encoding.UTF8.GetString(bodyBytes);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            stopwatch.Stop();
            response.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            response.ErrorMessage = $"Request timed out after {request.TimeoutSeconds} seconds.";
        }
        catch (TaskCanceledException)
        {
            stopwatch.Stop();
            response.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            response.ErrorMessage = "Request was cancelled.";
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            response.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            response.ErrorMessage = $"Request failed: {ex.Message}";
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            response.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            response.ErrorMessage = $"Unexpected error: {ex.Message}";
        }

        return response;
    }

    private static void ApplyHeaders(HttpRequestMessage httpRequest, HttpRequestModel request)
    {
        foreach (var header in request.Headers.Where(h => h.Enabled && !string.IsNullOrWhiteSpace(h.Key)))
        {
            // Some headers must be set on Content, not on Request
            if (IsContentHeader(header.Key))
                continue;

            httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    private static void ApplyAuthentication(HttpRequestMessage httpRequest, HttpRequestModel request)
    {
        switch (request.AuthType)
        {
            case AuthType.Bearer:
            case AuthType.OAuth2:
                if (!string.IsNullOrWhiteSpace(request.AuthToken))
                    httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.AuthToken);
                break;

            case AuthType.Basic:
                if (!string.IsNullOrWhiteSpace(request.AuthUsername))
                {
                    var credentials = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes($"{request.AuthUsername}:{request.AuthPassword}"));
                    httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                }
                break;

            case AuthType.ApiKey:
                if (!string.IsNullOrWhiteSpace(request.ApiKeyName) && !string.IsNullOrWhiteSpace(request.ApiKeyValue))
                {
                    if (request.ApiKeyLocation == ApiKeyLocation.Header)
                    {
                        httpRequest.Headers.TryAddWithoutValidation(request.ApiKeyName, request.ApiKeyValue);
                    }
                    // Query param is handled in GetFullUrl() of HttpRequestModel
                }
                break;
        }
    }

    private static void ApplyBody(HttpRequestMessage httpRequest, HttpRequestModel request)
    {
        if (request.BodyType == "none" || string.IsNullOrEmpty(request.Body))
            return;

        if (request.Method is "GET" or "HEAD")
            return;

        if (request.BodyType == "form-data" && request.FormData.Any(f => f.Enabled && !string.IsNullOrWhiteSpace(f.Key)))
        {
            var content = new MultipartFormDataContent();
            foreach (var item in request.FormData.Where(f => f.Enabled && !string.IsNullOrWhiteSpace(f.Key)))
            {
                content.Add(new StringContent(item.Value), item.Key);
            }
            httpRequest.Content = content;
        }
        else
        {
            httpRequest.Content = new StringContent(request.Body, Encoding.UTF8, request.ContentType);
        }
    }

    private static bool IsContentHeader(string headerName)
    {
        return headerName.Equals("Content-Type", StringComparison.OrdinalIgnoreCase) ||
               headerName.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) ||
               headerName.Equals("Content-Encoding", StringComparison.OrdinalIgnoreCase) ||
               headerName.Equals("Content-Language", StringComparison.OrdinalIgnoreCase) ||
               headerName.Equals("Content-Disposition", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<OAuth2TokenResult> GetOAuth2TokenAsync(HttpRequestModel request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.OAuth2TokenUrl))
            throw new ArgumentException("Token URL is required.");

        var data = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "client_credentials"),
            new("client_id", request.OAuth2ClientId),
            new("client_secret", request.OAuth2ClientSecret)
        };

        if (!string.IsNullOrWhiteSpace(request.OAuth2Scope))
        {
            data.Add(new KeyValuePair<string, string>("scope", request.OAuth2Scope));
        }

        using var content = new FormUrlEncodedContent(data);
        using var response = await httpClient.PostAsync(request.OAuth2TokenUrl, content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Failed to get token: {response.StatusCode} - {error}");
        }

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        
        string? accessToken = null;
        int? expiresIn = null;

        if (doc.RootElement.TryGetProperty("access_token", out var tokenProp))
        {
            accessToken = tokenProp.GetString();
        }

        if (doc.RootElement.TryGetProperty("expires_in", out var expiresProp) && expiresProp.TryGetInt32(out var expiresVal))
        {
            expiresIn = expiresVal;
        }

        if (string.IsNullOrEmpty(accessToken))
        {
            throw new Exception("Access token not found in response.");
        }

        return new OAuth2TokenResult(accessToken, expiresIn);
    }
}

public record OAuth2TokenResult(string AccessToken, int? ExpiresIn);

