using RobRequest.Shared.Models;

namespace RobRequest.Shared.Services;

public class HistoryService
{
    private readonly List<HistoryItem> _history = new();
    private int _maxItems = 1000;

    public event Action? OnHistoryChanged;

    public void SetMaxItems(int maxItems)
    {
        _maxItems = maxItems;
    }

    public Task<IReadOnlyList<HistoryItem>> GetHistoryAsync(int limit = 100)
    {
        var items = _history
            .OrderByDescending(h => h.Timestamp)
            .Take(limit)
            .ToList();

        return Task.FromResult<IReadOnlyList<HistoryItem>>(items);
    }

    public Task AddToHistoryAsync(HttpRequestModel request, HttpResponseModel response)
    {
        var item = new HistoryItem
        {
            Method = request.Method,
            Url = request.GetFullUrl(),
            StatusCode = response.StatusCode,
            ResponseTimeMs = response.ResponseTimeMs,
            Timestamp = DateTime.UtcNow,
            Request = request,
            Response = response
        };

        _history.Insert(0, item);

        // Trim history to max items
        while (_history.Count > _maxItems)
        {
            _history.RemoveAt(_history.Count - 1);
        }

        OnHistoryChanged?.Invoke();
        return Task.CompletedTask;
    }

    public Task ClearHistoryAsync()
    {
        _history.Clear();
        OnHistoryChanged?.Invoke();
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<HistoryItem>> SearchHistoryAsync(string query)
    {
        var items = _history
            .Where(h => h.Url.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        h.Method.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(h => h.Timestamp)
            .ToList();

        return Task.FromResult<IReadOnlyList<HistoryItem>>(items);
    }

    public Task RemoveFromHistoryAsync(string id)
    {
        _history.RemoveAll(h => h.Id == id);
        OnHistoryChanged?.Invoke();
        return Task.CompletedTask;
    }
}
