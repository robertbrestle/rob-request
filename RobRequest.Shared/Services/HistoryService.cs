using Microsoft.EntityFrameworkCore;
using RobRequest.Shared.Data;
using RobRequest.Shared.Models;

namespace RobRequest.Shared.Services;

public class HistoryService
{
    private readonly AppDbContext _db;
    private int _maxItems = 1000;

    public event Action? OnHistoryChanged;

    public HistoryService(AppDbContext db)
    {
        _db = db;
    }

    public void SetMaxItems(int maxItems)
    {
        _maxItems = maxItems;
    }

    public async Task<IReadOnlyList<HistoryItem>> GetHistoryAsync(int limit = 100)
    {
        return await _db.HistoryItems
            .OrderByDescending(h => h.Timestamp)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddToHistoryAsync(HttpRequestModel request, HttpResponseModel response)
    {
        // Snapshot request/response into fresh instances to avoid change-tracker
        // conflicts when the caller reuses the same object across multiple calls.
        var requestSnapshot = new HttpRequestModel
        {
            Id = Guid.NewGuid().ToString(),
            Method = request.Method,
            Url = request.Url,
            Headers = request.Headers.Select(h => new HeaderItem { Key = h.Key, Value = h.Value, Enabled = h.Enabled }).ToList(),
            QueryParams = request.QueryParams.Select(q => new QueryParamItem { Key = q.Key, Value = q.Value, Enabled = q.Enabled }).ToList(),
            FormData = request.FormData.Select(f => new FormDataItem { Key = f.Key, Value = f.Value, Enabled = f.Enabled, IsFile = f.IsFile, FileName = f.FileName, ContentType = f.ContentType }).ToList(),
            BodyType = request.BodyType,
            Body = request.Body,
            ContentType = request.ContentType,
            AuthType = request.AuthType,
            AuthToken = request.AuthToken,
            AuthUsername = request.AuthUsername,
            AuthPassword = request.AuthPassword,
            ApiKeyName = request.ApiKeyName,
            ApiKeyValue = request.ApiKeyValue,
            ApiKeyLocation = request.ApiKeyLocation,
            TimeoutSeconds = request.TimeoutSeconds,
            CreatedAt = request.CreatedAt
        };

        var responseSnapshot = new HttpResponseModel
        {
            StatusCode = response.StatusCode,
            StatusText = response.StatusText,
            Body = response.Body,
            Headers = response.Headers.Select(h => new HeaderItem { Key = h.Key, Value = h.Value, Enabled = h.Enabled }).ToList(),
            ContentType = response.ContentType,
            ResponseTimeMs = response.ResponseTimeMs,
            ResponseSizeBytes = response.ResponseSizeBytes,
            ReceivedAt = response.ReceivedAt,
            ErrorMessage = response.ErrorMessage
        };

        var item = new HistoryItem
        {
            Method = request.Method,
            Url = request.GetFullUrl(),
            StatusCode = response.StatusCode,
            ResponseTimeMs = response.ResponseTimeMs,
            Timestamp = DateTime.Now,
            Request = requestSnapshot,
            Response = responseSnapshot
        };

        _db.HistoryItems.Add(item);
        await _db.SaveChangesAsync();

        // Trim history to max items
        var count = await _db.HistoryItems.CountAsync();
        if (count > _maxItems)
        {
            var excess = await _db.HistoryItems
                .OrderBy(h => h.Timestamp)
                .Take(count - _maxItems)
                .ToListAsync();
            _db.HistoryItems.RemoveRange(excess);
            await _db.SaveChangesAsync();
        }

        OnHistoryChanged?.Invoke();
    }

    public async Task ClearHistoryAsync()
    {
        await _db.HistoryItems.ExecuteDeleteAsync();
        OnHistoryChanged?.Invoke();
    }

    public async Task<IReadOnlyList<HistoryItem>> SearchHistoryAsync(string query)
    {
        return await _db.HistoryItems
            .Where(h => h.Url.Contains(query) ||
                        h.Method.Contains(query))
            .OrderByDescending(h => h.Timestamp)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task RemoveFromHistoryAsync(string id)
    {
        await _db.HistoryItems.Where(h => h.Id == id).ExecuteDeleteAsync();
        OnHistoryChanged?.Invoke();
    }
}
