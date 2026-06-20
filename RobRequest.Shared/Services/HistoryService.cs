using Microsoft.EntityFrameworkCore;
using RobRequest.Shared.Data;
using RobRequest.Shared.Models;

namespace RobRequest.Shared.Services;

public class HistoryService
{
    private readonly AppDbContext _db;
    private readonly CurrentUserService _currentUser;
    private int _maxItems = 1000;

    public event Action? OnHistoryChanged;

    public HistoryService(AppDbContext db, CurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public void SetMaxItems(int maxItems)
    {
        _maxItems = maxItems;
    }

    public async Task<IReadOnlyList<HistoryItem>> GetHistoryAsync(int limit = 100)
    {
        return await _db.HistoryItems
            .Where(h => h.UserId == _currentUser.UserId)
            .OrderByDescending(h => h.Timestamp)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddToHistoryAsync(HttpRequestModel request, HttpResponseModel response)
    {
        // Snapshot request/response into fresh instances to avoid change-tracker
        // conflicts when the caller reuses the same object across multiple calls.
        var requestSnapshot = request.Clone();

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
            ErrorMessage = response.ErrorMessage,
            StackTrace = response.StackTrace
        };

        var item = new HistoryItem
        {
            Method = request.Method,
            Url = request.GetFullUrl(),
            StatusCode = response.StatusCode,
            ResponseTimeMs = response.ResponseTimeMs,
            UserId = _currentUser.UserId ?? string.Empty,
            Timestamp = DateTime.Now,
            Request = requestSnapshot,
            Response = responseSnapshot
        };

        _db.HistoryItems.Add(item);
        await _db.SaveChangesAsync();

        // Trim history to max items for this user
        var count = await _db.HistoryItems.Where(h => h.UserId == _currentUser.UserId).CountAsync();
        if (count > _maxItems)
        {
            var excess = await _db.HistoryItems
                .Where(h => h.UserId == _currentUser.UserId)
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
        await _db.HistoryItems.Where(h => h.UserId == _currentUser.UserId).ExecuteDeleteAsync();
        OnHistoryChanged?.Invoke();
    }

    public async Task<IReadOnlyList<HistoryItem>> SearchHistoryAsync(string query)
    {
        query = query.ToLower();
        return await _db.HistoryItems
            .Where(h => h.UserId == _currentUser.UserId)
            .Where(h => h.Url.ToLower().Contains(query) ||
                        h.Method.ToLower().Contains(query) ||
                        h.StatusCode.ToString().Contains(query))
            .OrderByDescending(h => h.Timestamp)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task RemoveFromHistoryAsync(string id)
    {
        await _db.HistoryItems.Where(h => h.Id == id && h.UserId == _currentUser.UserId).ExecuteDeleteAsync();
        OnHistoryChanged?.Invoke();
    }
}
