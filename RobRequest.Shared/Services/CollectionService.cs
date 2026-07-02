using Microsoft.EntityFrameworkCore;
using RobRequest.Shared.Data;
using RobRequest.Shared.Models;
using RobRequest.Shared.Models.Collections;
using RobRequest.Shared.Models.Requests;

namespace RobRequest.Shared.Services;

public class CollectionService(AppDbContext db, CurrentUserService currentUser)
{
    public event Action? OnCollectionsChanged;

    public async Task<List<Collection>> GetRootCollectionsAsync()
    {
        return await db.Collections
            .Where(c => c.UserId == currentUser.UserId)
            .Where(c => c.ParentId == null)
            .Include(c => c.Children)
            .Include(c => c.Requests)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Collection>> GetAllCollectionsAsync()
    {
        return await db.Collections
            .Where(c => c.UserId == currentUser.UserId)
            .Include(c => c.Requests)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Collection>> GetCollectionTreeAsync()
    {
        var all = await db.Collections
            .Where(c => c.UserId == currentUser.UserId)
            .Include(c => c.Requests.OrderBy(r => r.SortOrder).ThenBy(r => r.Name))
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync();

        var lookup = all.ToDictionary(c => c.Id);
        var roots = new List<Collection>();

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

    public async Task<Collection?> GetCollectionAsync(string id)
    {
        return await db.Collections
            .Include(c => c.Children)
            .Include(c => c.Requests.OrderBy(r => r.SortOrder).ThenBy(r => r.Name))
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Collection> CreateCollectionAsync(string name, string? parentId = null,
        string? description = null)
    {
        var maxSort = await db.Collections
            .Where(c => c.ParentId == parentId)
            .MaxAsync(c => (int?)c.SortOrder) ?? -1;

        var collection = new Collection
        {
            Name = name,
            Description = description,
            UserId = currentUser.UserId ?? string.Empty,
            ParentId = parentId,
            SortOrder = maxSort + 1,
            UpdatedAt = DateTime.Now
        };

        db.Collections.Add(collection);
        await db.SaveChangesAsync();
        OnCollectionsChanged?.Invoke();
        return collection;
    }

    public async Task UpdateCollectionAsync(Collection collection)
    {
        var existing = await db.Collections.FindAsync(collection.Id);
        if (existing == null) return;

        existing.Name = collection.Name;
        existing.Description = collection.Description;
        existing.ParentId = collection.ParentId;
        existing.SortOrder = collection.SortOrder;
        existing.UpdatedAt = DateTime.Now;

        await db.SaveChangesAsync();
        OnCollectionsChanged?.Invoke();
    }

    public async Task DeleteCollectionAsync(string id)
    {
        var collection = await db.Collections.FindAsync(id);
        if (collection == null) return;

        db.Collections.Remove(collection);
        await db.SaveChangesAsync();
        OnCollectionsChanged?.Invoke();
    }

    public async Task<CollectionRequest> AddRequestToCollectionAsync(string collectionId, string name,
        HttpRequestModel request)
    {
        var maxSort = await db.CollectionRequests
            .Where(r => r.CollectionId == collectionId)
            .MaxAsync(r => (int?)r.SortOrder) ?? -1;

        var requestSnapshot = request.Clone();

        var collectionRequest = new CollectionRequest
        {
            CollectionId = collectionId,
            Name = name,
            SortOrder = maxSort + 1,
            Request = requestSnapshot,
            UpdatedAt = DateTime.Now
        };

        db.CollectionRequests.Add(collectionRequest);
        await db.SaveChangesAsync();
        OnCollectionsChanged?.Invoke();
        return collectionRequest;
    }

    public async Task UpdateCollectionRequestAsync(CollectionRequest collectionRequest)
    {
        var existing = await db.CollectionRequests.FindAsync(collectionRequest.Id);
        if (existing == null) return;

        existing.Name = collectionRequest.Name;
        existing.CollectionId = collectionRequest.CollectionId;
        existing.SortOrder = collectionRequest.SortOrder;
        existing.Request = collectionRequest.Request;
        existing.UpdatedAt = DateTime.Now;

        await db.SaveChangesAsync();
        OnCollectionsChanged?.Invoke();
    }

    public async Task DeleteCollectionRequestAsync(string id)
    {
        var request = await db.CollectionRequests.FindAsync(id);
        if (request == null) return;

        db.CollectionRequests.Remove(request);
        await db.SaveChangesAsync();
        OnCollectionsChanged?.Invoke();
    }

    public async Task<CollectionRequest?> GetCollectionRequestAsync(string id)
    {
        return await db.CollectionRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<Collection>> SearchCollectionsAsync(string query)
    {
        query = query.ToLower();
        var allCollections = await db.Collections
            .Where(c => c.UserId == currentUser.UserId)
            .Include(c => c.Requests)
            .AsNoTracking()
            .ToListAsync();

        var matchingCollections = allCollections
            .Where(c => c.Name.ToLower().Contains(query) ||
                        (c.Description ?? "").ToLower().Contains(query))
            .ToList();

        var matchingRequests = await db.CollectionRequests
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
        var request = await db.CollectionRequests.FindAsync(requestId);
        if (request == null) return;

        var maxSort = await db.CollectionRequests
            .Where(r => r.CollectionId == targetCollectionId)
            .MaxAsync(r => (int?)r.SortOrder) ?? -1;

        request.CollectionId = targetCollectionId;
        request.SortOrder = maxSort + 1;
        request.UpdatedAt = DateTime.Now;

        await db.SaveChangesAsync();
        OnCollectionsChanged?.Invoke();
    }

    public async Task ClearAllCollectionsAsync()
    {
        var userCollectionIds = await db.Collections
            .Where(c => c.UserId == currentUser.UserId)
            .Select(c => c.Id)
            .ToListAsync();
        await db.CollectionRequests.Where(r => userCollectionIds.Contains(r.CollectionId)).ExecuteDeleteAsync();
        await db.Collections.Where(c => c.UserId == currentUser.UserId).ExecuteDeleteAsync();
        OnCollectionsChanged?.Invoke();
    }
}