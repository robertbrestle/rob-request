namespace RobRequest.Tests.Unit.Services;

public class ImportExportServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly ImportExportService _sut;

    public ImportExportServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        _db = new AppDbContext(options);
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();
        _sut = new ImportExportService(_db);
    }

    public void Dispose()
    {
        _db.Database.CloseConnection();
        _db.Dispose();
    }

    // --- Export Tests ---

    [Fact]
    public async Task BuildExportAsync_EmptySelections_ReturnsEmptyExport()
    {
        var export = await _sut.BuildExportAsync();

        export.FormatVersion.Should().Be("1");
        export.AppVersion.Should().NotBeNullOrEmpty();
        export.Collections.Should().BeNull();
        export.Environments.Should().BeNull();
        export.History.Should().BeNull();
    }

    [Fact]
    public async Task BuildExportAsync_ExportsSelectedCollections()
    {
        var collection = new CollectionModel
        {
            Id = "col-1",
            Name = "Test Collection",
            Description = "A test",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        _db.Collections.Add(collection);

        var request = new CollectionRequestModel
        {
            Id = "req-1",
            CollectionId = "col-1",
            Name = "GET Users",
            SortOrder = 0,
            Request = new HttpRequestModel { Method = "GET", Url = "https://api.example.com/users" },
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        _db.CollectionRequests.Add(request);
        await _db.SaveChangesAsync();

        var export = await _sut.BuildExportAsync(collectionIds: new List<string> { "col-1" });

        export.Collections.Should().HaveCount(1);
        export.Collections![0].Name.Should().Be("Test Collection");
        export.Collections[0].Requests.Should().HaveCount(1);
        export.Collections[0].Requests[0].Name.Should().Be("GET Users");
    }

    [Fact]
    public async Task BuildExportAsync_IncludesSubtree()
    {
        _db.Collections.Add(new CollectionModel { Id = "parent", Name = "Parent" });
        _db.Collections.Add(new CollectionModel { Id = "child", Name = "Child", ParentId = "parent" });
        _db.Collections.Add(new CollectionModel { Id = "grandchild", Name = "Grandchild", ParentId = "child" });
        _db.Collections.Add(new CollectionModel { Id = "other", Name = "Other" });
        await _db.SaveChangesAsync();

        var export = await _sut.BuildExportAsync(collectionIds: new List<string> { "parent" });

        export.Collections.Should().HaveCount(3);
        export.Collections!.Select(c => c.Name).Should().Contain(new[] { "Parent", "Child", "Grandchild" });
        export.Collections.Select(c => c.Name).Should().NotContain("Other");
    }

    [Fact]
    public async Task BuildExportAsync_ExportsSelectedEnvironments()
    {
        _db.Environments.Add(new EnvironmentModel
        {
            Id = "env-1",
            Name = "Dev",
            Variables = new List<EnvironmentVariable>
            {
                new() { Key = "BASE_URL", Value = "http://localhost:5000", Enabled = true }
            }
        });
        _db.Environments.Add(new EnvironmentModel { Id = "env-2", Name = "Prod" });
        await _db.SaveChangesAsync();

        var export = await _sut.BuildExportAsync(environmentIds: new List<string> { "env-1" });

        export.Environments.Should().HaveCount(1);
        export.Environments![0].Name.Should().Be("Dev");
        export.Environments[0].Variables.Should().HaveCount(1);
        export.Environments[0].Variables[0].Key.Should().Be("BASE_URL");
    }

    [Fact]
    public async Task BuildExportAsync_ExportsSelectedHistory()
    {
        _db.HistoryItems.Add(new HistoryItem
        {
            Id = "h-1",
            Method = "GET",
            Url = "https://example.com",
            StatusCode = 200,
            ResponseTimeMs = 100,
            Timestamp = DateTime.Now
        });
        _db.HistoryItems.Add(new HistoryItem { Id = "h-2", Method = "POST", Url = "https://other.com" });
        await _db.SaveChangesAsync();

        var export = await _sut.BuildExportAsync(historyIds: new List<string> { "h-1" });

        export.History.Should().HaveCount(1);
        export.History![0].Method.Should().Be("GET");
        export.History[0].Url.Should().Be("https://example.com");
    }

    // --- Serialization Round-Trip ---

    [Fact]
    public async Task SerializeAndDeserialize_RoundTrip()
    {
        _db.Collections.Add(new CollectionModel { Id = "col-1", Name = "My API" });
        _db.Environments.Add(new EnvironmentModel
        {
            Id = "env-1",
            Name = "Dev",
            Variables = new List<EnvironmentVariable>
            {
                new() { Key = "URL", Value = "http://localhost", Enabled = true, IsSecret = false }
            }
        });
        await _db.SaveChangesAsync();

        var export = await _sut.BuildExportAsync(
            collectionIds: new List<string> { "col-1" },
            environmentIds: new List<string> { "env-1" });

        var json = _sut.SerializeExport(export);
        var deserialized = _sut.DeserializeExport(json);

        deserialized.Should().NotBeNull();
        deserialized!.FormatVersion.Should().Be("1");
        deserialized.Collections.Should().HaveCount(1);
        deserialized.Collections![0].Name.Should().Be("My API");
        deserialized.Environments.Should().HaveCount(1);
        deserialized.Environments![0].Name.Should().Be("Dev");
        deserialized.Environments[0].Variables.Should().HaveCount(1);
    }

    // --- Import Tests ---

    [Fact]
    public async Task ImportAsync_RejectsUnsupportedFormatVersion()
    {
        var data = new RobRequestExport { FormatVersion = "99" };

        var result = await _sut.ImportAsync(data);

        result.Success.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Contain("Unsupported format version");
    }

    [Fact]
    public async Task ImportAsync_ImportsCollectionsWithNewIds()
    {
        var data = new RobRequestExport
        {
            FormatVersion = "1",
            Collections = new List<ExportedCollection>
            {
                new()
                {
                    Id = "old-id",
                    Name = "Imported Collection",
                    Requests = new List<ExportedCollectionRequest>
                    {
                        new()
                        {
                            Id = "old-req",
                            Name = "GET Test",
                            Request = new HttpRequestModel { Method = "GET", Url = "https://test.com" }
                        }
                    }
                }
            }
        };

        var result = await _sut.ImportAsync(data);

        result.Success.Should().BeTrue();
        result.CollectionsImported.Should().Be(1);
        result.RequestsImported.Should().Be(1);

        var collections = await _db.Collections.Include(c => c.Requests).ToListAsync();
        collections.Should().HaveCount(1);
        collections[0].Id.Should().NotBe("old-id");
        collections[0].Name.Should().Be("Imported Collection");
        collections[0].Requests.Should().HaveCount(1);
        collections[0].Requests[0].Id.Should().NotBe("old-req");
    }

    [Fact]
    public async Task ImportAsync_AppendsTimestampOnCollectionNameConflict()
    {
        _db.Collections.Add(new CollectionModel { Id = "existing", Name = "My API" });
        await _db.SaveChangesAsync();

        var data = new RobRequestExport
        {
            FormatVersion = "1",
            Collections = new List<ExportedCollection>
            {
                new() { Id = "new-id", Name = "My API" }
            }
        };

        var result = await _sut.ImportAsync(data);

        result.Success.Should().BeTrue();
        var collections = await _db.Collections.ToListAsync();
        collections.Should().HaveCount(2);
        var imported = collections.First(c => c.Id != "existing");
        imported.Name.Should().StartWith("My API (");
        imported.Name.Should().MatchRegex(@"^My API \(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\)$");
    }

    [Fact]
    public async Task ImportAsync_DoesNotRenameSubCollections()
    {
        _db.Collections.Add(new CollectionModel { Id = "existing-child", Name = "Requests" });
        await _db.SaveChangesAsync();

        var data = new RobRequestExport
        {
            FormatVersion = "1",
            Collections = new List<ExportedCollection>
            {
                new() { Id = "parent", Name = "API Project" },
                new() { Id = "child", Name = "Requests", ParentId = "parent" }
            }
        };

        var result = await _sut.ImportAsync(data);

        result.Success.Should().BeTrue();
        result.CollectionsImported.Should().Be(2);
        var imported = await _db.Collections.Where(c => c.ParentId != null).ToListAsync();
        var importedChild = imported.FirstOrDefault(c => c.Id != "existing-child");
        importedChild.Should().NotBeNull();
        importedChild!.Name.Should().Be("Requests");
    }

    [Fact]
    public async Task ImportAsync_RemapsParentIds()
    {
        var data = new RobRequestExport
        {
            FormatVersion = "1",
            Collections = new List<ExportedCollection>
            {
                new() { Id = "old-parent", Name = "Parent" },
                new() { Id = "old-child", Name = "Child", ParentId = "old-parent" }
            }
        };

        var result = await _sut.ImportAsync(data);

        result.Success.Should().BeTrue();
        var collections = await _db.Collections.ToListAsync();
        var parent = collections.First(c => c.ParentId == null);
        var child = collections.First(c => c.ParentId != null);
        child.ParentId.Should().Be(parent.Id);
        child.ParentId.Should().NotBe("old-parent");
    }

    [Fact]
    public async Task ImportAsync_AppendsTimestampOnEnvironmentNameConflict()
    {
        _db.Environments.Add(new EnvironmentModel { Id = "existing", Name = "Dev" });
        await _db.SaveChangesAsync();

        var data = new RobRequestExport
        {
            FormatVersion = "1",
            Environments = new List<ExportedEnvironment>
            {
                new()
                {
                    Id = "new-env",
                    Name = "Dev",
                    Variables = new List<EnvironmentVariable>
                    {
                        new() { Key = "URL", Value = "http://localhost", Enabled = true }
                    }
                }
            }
        };

        var result = await _sut.ImportAsync(data);

        result.Success.Should().BeTrue();
        result.EnvironmentsImported.Should().Be(1);
        var envs = await _db.Environments.ToListAsync();
        envs.Should().HaveCount(2);
        var imported = envs.First(e => e.Id != "existing");
        imported.Name.Should().StartWith("Dev (");
        imported.Variables.Should().HaveCount(1);
    }

    [Fact]
    public async Task ImportAsync_ImportsHistoryWithNewIds()
    {
        var data = new RobRequestExport
        {
            FormatVersion = "1",
            History = new List<ExportedHistoryItem>
            {
                new()
                {
                    Method = "GET",
                    Url = "https://example.com",
                    StatusCode = 200,
                    ResponseTimeMs = 50,
                    Timestamp = DateTime.Now,
                    Request = new HttpRequestModel { Method = "GET", Url = "https://example.com" },
                    Response = new HttpResponseModel { StatusCode = 200, StatusText = "OK" }
                }
            }
        };

        var result = await _sut.ImportAsync(data);

        result.Success.Should().BeTrue();
        result.HistoryImported.Should().Be(1);
        var items = await _db.HistoryItems.ToListAsync();
        items.Should().HaveCount(1);
        items[0].Method.Should().Be("GET");
    }

    [Fact]
    public async Task ImportAsync_PartialExport_OnlyEnvironments()
    {
        var data = new RobRequestExport
        {
            FormatVersion = "1",
            Environments = new List<ExportedEnvironment>
            {
                new() { Id = "e1", Name = "Staging" }
            }
        };

        var result = await _sut.ImportAsync(data);

        result.Success.Should().BeTrue();
        result.CollectionsImported.Should().Be(0);
        result.EnvironmentsImported.Should().Be(1);
        result.HistoryImported.Should().Be(0);
    }

    [Fact]
    public void ImportResult_Summary_DescribesImport()
    {
        var result = new ImportResult
        {
            CollectionsImported = 2,
            RequestsImported = 5,
            EnvironmentsImported = 1,
            HistoryImported = 10
        };

        result.Summary.Should().Contain("2 collection(s)");
        result.Summary.Should().Contain("5 request(s)");
        result.Summary.Should().Contain("1 environment(s)");
        result.Summary.Should().Contain("10 history item(s)");
    }

    [Fact]
    public void ImportResult_Summary_EmptyWhenNothingImported()
    {
        var result = new ImportResult();
        result.Summary.Should().Be("Nothing imported.");
    }
}
