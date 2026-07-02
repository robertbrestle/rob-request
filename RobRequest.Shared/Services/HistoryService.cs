using Microsoft.EntityFrameworkCore;
using RobRequest.Shared.Data;
using RobRequest.Shared.Models;
using RobRequest.Shared.Models.History;
using RobRequest.Shared.Models.Requests;

namespace RobRequest.Shared.Services;

public class HistoryService(AppDbContext db, SettingsService settingsService, CurrentUserService currentUser)
{
    public event Action? OnHistoryChanged;

    public async Task<IReadOnlyList<HistoryItem>> GetHistoryAsync(int limit = 100)
    {
        return await db.HistoryItems
            .Where(h => h.UserId == currentUser.UserId)
            .OrderByDescending(h => h.Timestamp)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddToHistoryAsync(HttpRequestModel request, HttpResponseModel response)
    {
        var settings = await settingsService.GetSettingsAsync();

        // Snapshot request/response into fresh instances to avoid change-tracker
        // conflicts when the caller reuses the same object across multiple calls.
        var requestSnapshot = request.Clone();

        var responseSnapshot = new HttpResponseModel
        {
            StatusCode = response.StatusCode,
            StatusText = response.StatusText,
            Body = response.Body,
            Headers = response.Headers.Select(h => new KeyValueEntry { Key = h.Key, Value = h.Value, Enabled = h.Enabled })
                .ToList(),
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
            UserId = currentUser.UserId ?? string.Empty,
            Timestamp = DateTime.Now,
            Request = requestSnapshot,
            Response = responseSnapshot
        };

        db.HistoryItems.Add(item);
        await db.SaveChangesAsync();

        // Trim history to max items for this user
        var count = await db.HistoryItems.Where(h => h.UserId == currentUser.UserId).CountAsync();
        if (count > settings.MaxHistoryItems)
        {
            var excess = await db.HistoryItems
                .Where(h => h.UserId == currentUser.UserId)
                .OrderBy(h => h.Timestamp)
                .Take(count - settings.MaxHistoryItems)
                .ToListAsync();
            db.HistoryItems.RemoveRange(excess);
            await db.SaveChangesAsync();
        }

        OnHistoryChanged?.Invoke();
    }

    public async Task ClearHistoryAsync()
    {
        await db.HistoryItems.Where(h => h.UserId == currentUser.UserId).ExecuteDeleteAsync();
        OnHistoryChanged?.Invoke();
    }
    
    public async Task ClearUserHistoryAsync(string id)
    {
        await db.HistoryItems.Where(h => h.UserId == id).ExecuteDeleteAsync();
        OnHistoryChanged?.Invoke();
    }

    public async Task<IReadOnlyList<HistoryItem>> SearchHistoryAsync(string query)
    {
        query = query.ToLower();
        return await db.HistoryItems
            .Where(h => h.UserId == currentUser.UserId)
            .Where(h => h.Url.ToLower().Contains(query) ||
                        h.Method.ToLower().Contains(query) ||
                        h.StatusCode.ToString().Contains(query))
            .OrderByDescending(h => h.Timestamp)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task RemoveFromHistoryAsync(string id)
    {
        await db.HistoryItems.Where(h => h.Id == id && h.UserId == currentUser.UserId).ExecuteDeleteAsync();
        OnHistoryChanged?.Invoke();
    }
}