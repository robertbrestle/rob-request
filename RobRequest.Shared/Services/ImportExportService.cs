using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RobRequest.Shared.Data;
using RobRequest.Shared.Models;

namespace RobRequest.Shared.Services;

public class ImportExportService
{
    private readonly AppDbContext _db;
    private readonly CurrentUserService _currentUser;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private static readonly JsonSerializerOptions DeserializeOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public ImportExportService(AppDbContext db, CurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<RobRequestExport> BuildExportAsync(
        List<string>? collectionIds = null,
        List<string>? environmentIds = null,
        List<string>? historyIds = null)
    {
        var export = new RobRequestExport
        {
            FormatVersion = "1",
            AppVersion = AppInfo.Version,
            ExportedAt = DateTime.UtcNow
        };

        if (collectionIds is { Count: > 0 })
        {
            export.Collections = await BuildCollectionExportAsync(collectionIds);
        }

        if (environmentIds is { Count: > 0 })
        {
            export.Environments = await BuildEnvironmentExportAsync(environmentIds);
        }

        if (historyIds is { Count: > 0 })
        {
            export.History = await BuildHistoryExportAsync(historyIds);
        }

        return export;
    }

    public string SerializeExport(RobRequestExport export)
    {
        return JsonSerializer.Serialize(export, JsonOptions);
    }

    public RobRequestExport? DeserializeExport(string json)
    {
        return JsonSerializer.Deserialize<RobRequestExport>(json, DeserializeOptions);
    }

    public async Task<ImportResult> ImportAsync(RobRequestExport data)
    {
        var result = new ImportResult();

        if (data.FormatVersion != "1")
        {
            result.Errors.Add($"Unsupported format version: {data.FormatVersion}. Only version \"1\" is supported.");
            return result;
        }

        if (data.Collections is { Count: > 0 })
        {
            await ImportCollectionsAsync(data.Collections, result);
        }

        if (data.Environments is { Count: > 0 })
        {
            await ImportEnvironmentsAsync(data.Environments, result);
        }

        if (data.History is { Count: > 0 })
        {
            await ImportHistoryAsync(data.History, result);
        }

        return result;
    }

    private async Task<List<ExportedCollection>> BuildCollectionExportAsync(List<string> collectionIds)
    {
        // Get all collections to traverse subtrees (scoped to current user)
        var allCollections = await _db.Collections
            .Where(c => c.UserId == _currentUser.UserId)
            .Include(c => c.Requests)
            .AsNoTracking()
            .ToListAsync();

        var lookup = allCollections.ToDictionary(c => c.Id);
        var idsToExport = new HashSet<string>();

        // For each selected collection, include it and all descendants
        foreach (var id in collectionIds)
        {
            CollectSubtreeIds(id, allCollections, idsToExport);
        }

        return allCollections
            .Where(c => idsToExport.Contains(c.Id))
            .OrderBy(c => c.SortOrder)
            .Select(c => new ExportedCollection
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ParentId = c.ParentId,
                SortOrder = c.SortOrder,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                Requests = c.Requests.OrderBy(r => r.SortOrder).Select(r => new ExportedCollectionRequest
                {
                    Id = r.Id,
                    Name = r.Name,
                    SortOrder = r.SortOrder,
                    Request = r.Request,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                }).ToList()
            })
            .ToList();
    }

    private static void CollectSubtreeIds(string rootId, List<CollectionModel> allCollections, HashSet<string> ids)
    {
        if (!ids.Add(rootId)) return;

        foreach (var child in allCollections.Where(c => c.ParentId == rootId))
        {
            CollectSubtreeIds(child.Id, allCollections, ids);
        }
    }

    private async Task<List<ExportedEnvironment>> BuildEnvironmentExportAsync(List<string> environmentIds)
    {
        var environments = await _db.Environments
            .Where(e => e.UserId == _currentUser.UserId && environmentIds.Contains(e.Id))
            .AsNoTracking()
            .ToListAsync();

        return environments.OrderBy(e => e.SortOrder).Select(e => new ExportedEnvironment
        {
            Id = e.Id,
            Name = e.Name,
            Description = e.Description,
            SortOrder = e.SortOrder,
            Variables = e.Variables,
            Auth = e.Auth,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        }).ToList();
    }

    private async Task<List<ExportedHistoryItem>> BuildHistoryExportAsync(List<string> historyIds)
    {
        var items = await _db.HistoryItems
            .Where(h => h.UserId == _currentUser.UserId && historyIds.Contains(h.Id))
            .AsNoTracking()
            .ToListAsync();

        return items.OrderByDescending(h => h.Timestamp).Select(h => new ExportedHistoryItem
        {
            Method = h.Method,
            Url = h.Url,
            StatusCode = h.StatusCode,
            ResponseTimeMs = h.ResponseTimeMs,
            Timestamp = h.Timestamp,
            Request = h.Request,
            Response = h.Response
        }).ToList();
    }

    private async Task ImportCollectionsAsync(List<ExportedCollection> collections, ImportResult result)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var existingNames = await _db.Collections
            .Where(c => c.UserId == _currentUser.UserId && c.ParentId == null)
            .Select(c => c.Name)
            .ToListAsync();
        var existingNameSet = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);

        // Build old-to-new ID map
        var idMap = new Dictionary<string, string>();
        foreach (var col in collections)
        {
            idMap[col.Id] = Guid.NewGuid().ToString();
        }

        // Determine which collections are top-level within the export
        // A collection is top-level if its ParentId is null or its ParentId is not in the export set
        var exportedIds = new HashSet<string>(collections.Select(c => c.Id));

        foreach (var col in collections)
        {
            var newId = idMap[col.Id];
            string? newParentId = null;

            if (col.ParentId != null && idMap.TryGetValue(col.ParentId, out var mappedParent))
            {
                newParentId = mappedParent;
            }

            var isTopLevel = newParentId == null;
            var name = col.Name;

            // Only rename top-level collections on conflict
            if (isTopLevel && existingNameSet.Contains(name))
            {
                name = $"{name} ({timestamp})";
            }

            var newCollection = new CollectionModel
            {
                Id = newId,
                Name = name,
                Description = col.Description,
                UserId = _currentUser.UserId ?? string.Empty,
                ParentId = newParentId,
                SortOrder = col.SortOrder,
                CreatedAt = col.CreatedAt,
                UpdatedAt = col.UpdatedAt
            };

            _db.Collections.Add(newCollection);

            foreach (var req in col.Requests)
            {
                var newRequest = new CollectionRequestModel
                {
                    Id = Guid.NewGuid().ToString(),
                    CollectionId = newId,
                    Name = req.Name,
                    SortOrder = req.SortOrder,
                    Request = CloneRequest(req.Request),
                    CreatedAt = req.CreatedAt,
                    UpdatedAt = req.UpdatedAt
                };

                _db.CollectionRequests.Add(newRequest);
                result.RequestsImported++;
            }

            result.CollectionsImported++;
        }

        await _db.SaveChangesAsync();
    }

    private async Task ImportEnvironmentsAsync(List<ExportedEnvironment> environments, ImportResult result)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var existingNames = await _db.Environments
            .Where(e => e.UserId == _currentUser.UserId)
            .Select(e => e.Name)
            .ToListAsync();
        var existingNameSet = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);

        foreach (var env in environments)
        {
            var name = env.Name;
            if (existingNameSet.Contains(name))
            {
                name = $"{name} ({timestamp})";
            }

            var newEnv = new EnvironmentModel
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Description = env.Description,
                UserId = _currentUser.UserId ?? string.Empty,
                SortOrder = env.SortOrder,
                Variables = env.Variables.Select(v => new EnvironmentVariable
                {
                    Key = v.Key,
                    Value = v.Value,
                    IsSecret = v.IsSecret,
                    Enabled = v.Enabled
                }).ToList(),
                Auth = env.Auth.Clone(),
                CreatedAt = env.CreatedAt,
                UpdatedAt = env.UpdatedAt
            };

            _db.Environments.Add(newEnv);
            result.EnvironmentsImported++;
        }

        await _db.SaveChangesAsync();
    }

    private async Task ImportHistoryAsync(List<ExportedHistoryItem> historyItems, ImportResult result)
    {
        foreach (var item in historyItems)
        {
            var newItem = new HistoryItem
            {
                Id = Guid.CreateVersion7().ToString(),
                Method = item.Method,
                Url = item.Url,
                StatusCode = item.StatusCode,
                ResponseTimeMs = item.ResponseTimeMs,
                UserId = _currentUser.UserId ?? string.Empty,
                Timestamp = item.Timestamp,
                Request = item.Request != null ? CloneRequest(item.Request) : null,
                Response = item.Response != null ? CloneResponse(item.Response) : null
            };

            _db.HistoryItems.Add(newItem);
            result.HistoryImported++;
        }

        await _db.SaveChangesAsync();
    }

    private static HttpRequestModel CloneRequest(HttpRequestModel source)
    {
        return source.Clone();
    }

    private static HttpResponseModel CloneResponse(HttpResponseModel source)
    {
        return new HttpResponseModel
        {
            StatusCode = source.StatusCode,
            StatusText = source.StatusText,
            Body = source.Body,
            Headers = source.Headers.Select(h => new HeaderItem { Key = h.Key, Value = h.Value, Enabled = h.Enabled }).ToList(),
            ContentType = source.ContentType,
            ResponseTimeMs = source.ResponseTimeMs,
            ResponseSizeBytes = source.ResponseSizeBytes,
            ReceivedAt = source.ReceivedAt,
            ErrorMessage = source.ErrorMessage
        };
    }
}

public class ImportResult
{
    public int CollectionsImported { get; set; }
    public int RequestsImported { get; set; }
    public int EnvironmentsImported { get; set; }
    public int HistoryImported { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool Success => Errors.Count == 0;

    public string Summary
    {
        get
        {
            var parts = new List<string>();
            if (CollectionsImported > 0) parts.Add($"{CollectionsImported} collection(s)");
            if (RequestsImported > 0) parts.Add($"{RequestsImported} request(s)");
            if (EnvironmentsImported > 0) parts.Add($"{EnvironmentsImported} environment(s)");
            if (HistoryImported > 0) parts.Add($"{HistoryImported} history item(s)");
            return parts.Count > 0 ? $"Imported {string.Join(", ", parts)}." : "Nothing imported.";
        }
    }
}
