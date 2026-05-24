using Microsoft.EntityFrameworkCore;
using RobRequest.Shared.Data;
using RobRequest.Shared.Models;

namespace RobRequest.Shared.Services;

public class CollectionService
{
    private readonly AppDbContext _db;
    private readonly CurrentUserService _currentUser;

    public event Action? OnCollectionsChanged;

    public CollectionService(AppDbContext db, CurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<CollectionModel>> GetRootCollectionsAsync()
    {
        return await _db.Collections
            .Where(c => c.UserId == _currentUser.UserId)
            .Where(c => c.ParentId == null)
            .Include(c => c.Children)
            .Include(c => c.Requests)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<CollectionModel>> GetAllCollectionsAsync()
    {
        return await _db.Collections
            .Where(c => c.UserId == _currentUser.UserId)
            .Include(c => c.Requests)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<CollectionModel>> GetCollectionTreeAsync()
    {
        var all = await _db.Collections
            .Where(c => c.UserId == _currentUser.UserId)
            .Include(c => c.Requests.OrderBy(r => r.SortOrder).ThenBy(r => r.Name))
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync();

        var lookup = all.ToDictionary(c => c.Id);
        var roots = new List<CollectionModel>();

        foreach (var c in all)
        {
            if (c.ParentId != null && lookup.TryGetValue(c.ParentId, out var parent))
            {
                parent.Children.Add(c);
            }
            else
            {
                roots.Add(c);
            }
        }

        return roots;
    }

    public async Task<CollectionModel?> GetCollectionAsync(string id)
    {
        return await _db.Collections
            .Include(c => c.Children)
            .Include(c => c.Requests.OrderBy(r => r.SortOrder).ThenBy(r => r.Name))
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CollectionModel> CreateCollectionAsync(string name, string? parentId = null, string? description = null)
    {
        var maxSort = await _db.Collections
            .Where(c => c.ParentId == parentId)
            .MaxAsync(c => (int?)c.SortOrder) ?? -1;

        var collection = new CollectionModel
        {
            Name = name,
            Description = description,
            UserId = _currentUser.UserId ?? string.Empty,
            ParentId = parentId,
            SortOrder = maxSort + 1,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _db.Collections.Add(collection);
        await _db.SaveChangesAsync();
        OnCollectionsChanged?.Invoke();
        return collection;
    }

    public async Task UpdateCollectionAsync(CollectionModel collection)
    {
        var existing = await _db.Collections.FindAsync(collection.Id);
        if (existing == null) return;

        existing.Name = collection.Name;
        existing.Description = collection.Description;
        existing.ParentId = collection.ParentId;
        existing.SortOrder = collection.SortOrder;
        existing.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();
        OnCollectionsChanged?.Invoke();
    }

    public async Task DeleteCollectionAsync(string id)
    {
        var collection = await _db.Collections.FindAsync(id);
        if (collection == null) return;

        _db.Collections.Remove(collection);
        await _db.SaveChangesAsync();
        OnCollectionsChanged?.Invoke();
    }

    public async Task<CollectionRequestModel> AddRequestToCollectionAsync(string collectionId, string name, HttpRequestModel request)
    {
        var maxSort = await _db.CollectionRequests
            .Where(r => r.CollectionId == collectionId)
            .MaxAsync(r => (int?)r.SortOrder) ?? -1;

        var requestSnapshot = request.Clone();

        var collectionRequest = new CollectionRequestModel
        {
            CollectionId = collectionId,
            Name = name,
            SortOrder = maxSort + 1,
            Request = requestSnapshot,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _db.CollectionRequests.Add(collectionRequest);
        await _db.SaveChangesAsync();
        OnCollectionsChanged?.Invoke();
        return collectionRequest;
    }

    public async Task UpdateCollectionRequestAsync(CollectionRequestModel collectionRequest)
    {
        var existing = await _db.CollectionRequests.FindAsync(collectionRequest.Id);
        if (existing == null) return;

        existing.Name = collectionRequest.Name;
        existing.CollectionId = collectionRequest.CollectionId;
        existing.SortOrder = collectionRequest.SortOrder;
        existing.Request = collectionRequest.Request;
        existing.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();
        OnCollectionsChanged?.Invoke();
    }

    public async Task DeleteCollectionRequestAsync(string id)
    {
        var request = await _db.CollectionRequests.FindAsync(id);
        if (request == null) return;

        _db.CollectionRequests.Remove(request);
        await _db.SaveChangesAsync();
        OnCollectionsChanged?.Invoke();
    }

    public async Task<CollectionRequestModel?> GetCollectionRequestAsync(string id)
    {
        return await _db.CollectionRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<CollectionModel>> SearchCollectionsAsync(string query)
    {
        query = query.ToLower();
        var allCollections = await _db.Collections
            .Where(c => c.UserId == _currentUser.UserId)
            .Include(c => c.Requests)
            .AsNoTracking()
            .ToListAsync();

        var matchingCollections = allCollections
            .Where(c => c.Name.ToLower().Contains(query) ||
                        (c.Description ?? "").ToLower().Contains(query))
            .ToList();

        var matchingRequests = await _db.CollectionRequests
            .Where(r => r.Name.ToLower().Contains(query) ||
                        r.Request.Url.ToLower().Contains(query))
            .AsNoTracking()
            .ToListAsync();

        var collectionIdsFromRequests = matchingRequests
            .Select(r => r.CollectionId)
            .Distinct();

        foreach (var colId in collectionIdsFromRequests)
        {
            if (matchingCollections.All(c => c.Id != colId))
            {
                var col = allCollections.FirstOrDefault(c => c.Id == colId);
                if (col != null) matchingCollections.Add(col);
            }
        }

        return matchingCollections.OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToList();
    }

    public async Task MoveRequestToCollectionAsync(string requestId, string targetCollectionId)
    {
        var request = await _db.CollectionRequests.FindAsync(requestId);
        if (request == null) return;

        var maxSort = await _db.CollectionRequests
            .Where(r => r.CollectionId == targetCollectionId)
            .MaxAsync(r => (int?)r.SortOrder) ?? -1;

        request.CollectionId = targetCollectionId;
        request.SortOrder = maxSort + 1;
        request.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();
        OnCollectionsChanged?.Invoke();
    }

    public async Task ClearAllCollectionsAsync()
    {
        var userCollectionIds = await _db.Collections
            .Where(c => c.UserId == _currentUser.UserId)
            .Select(c => c.Id)
            .ToListAsync();
        await _db.CollectionRequests.Where(r => userCollectionIds.Contains(r.CollectionId)).ExecuteDeleteAsync();
        await _db.Collections.Where(c => c.UserId == _currentUser.UserId).ExecuteDeleteAsync();
        OnCollectionsChanged?.Invoke();
    }
}
