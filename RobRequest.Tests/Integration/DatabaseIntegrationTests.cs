namespace RobRequest.Tests.Integration;

public class DatabaseIntegrationTests : IDisposable
{
    private readonly AppDbContext _db;

    public DatabaseIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        _db = new AppDbContext(options);
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _db.Database.CloseConnection();
        _db.Dispose();
    }

    [Fact]
    public async Task HistoryItem_PersistsAndReloads()
    {
        var service = new HistoryService(_db);
        var request = new HttpRequestModel
        {
            Method = "POST",
            Url = "https://api.example.com/data",
            Body = "{\"key\":\"value\"}",
            Headers = [new HeaderItem { Key = "Content-Type", Value = "application/json" }]
        };
        var response = new HttpResponseModel
        {
            StatusCode = 201,
            StatusText = "Created",
            Body = "{\"id\":1}",
            ResponseTimeMs = 150,
            ResponseSizeBytes = 8
        };

        await service.AddToHistoryAsync(request, response);

        // Verify via a fresh query
        var history = await service.GetHistoryAsync();
        history.Should().HaveCount(1);

        var item = history[0];
        item.Method.Should().Be("POST");
        item.Url.Should().Be("https://api.example.com/data");
        item.StatusCode.Should().Be(201);
        item.ResponseTimeMs.Should().Be(150);
        item.Request.Should().NotBeNull();
        item.Request!.Body.Should().Be("{\"key\":\"value\"}");
        item.Request.Headers.Should().HaveCount(1);
        item.Request.Headers[0].Key.Should().Be("Content-Type");
        item.Response.Should().NotBeNull();
        item.Response!.StatusText.Should().Be("Created");
        item.Response.ResponseSizeBytes.Should().Be(8);
    }

    [Fact]
    public async Task HistoryItem_RemoveById_DeletesOnlyTarget()
    {
        var service = new HistoryService(_db);
        var response = new HttpResponseModel { StatusCode = 200 };

        await service.AddToHistoryAsync(new HttpRequestModel { Url = "https://keep.com" }, response);
        await service.AddToHistoryAsync(new HttpRequestModel { Url = "https://remove.com" }, response);

        var all = await service.GetHistoryAsync();
        all.Should().HaveCount(2);

        var toRemove = all.First(h => h.Url == "https://remove.com");
        await service.RemoveFromHistoryAsync(toRemove.Id);

        var remaining = await service.GetHistoryAsync();
        remaining.Should().HaveCount(1);
        remaining[0].Url.Should().Be("https://keep.com");
    }

    [Fact]
    public async Task UserSettings_DefaultsCreatedOnFirstAccess()
    {
        var service = new SettingsService(_db);

        var settings = await service.GetSettingsAsync();

        settings.Should().NotBeNull();
        settings.Id.Should().Be("default");
        settings.DarkMode.Should().BeTrue();
        settings.DefaultTimeoutSeconds.Should().Be(30);
        settings.MaxHistoryItems.Should().Be(1000);
    }

    [Fact]
    public async Task UserSettings_UpdatePersistsAcrossReads()
    {
        var service = new SettingsService(_db);

        await service.GetSettingsAsync();
        await service.UpdateSettingsAsync(new UserSettings
        {
            DarkMode = false,
            DefaultTimeoutSeconds = 60,
            MaxHistoryItems = 500,
            AutoFormatJson = false,
            FollowRedirects = false,
            ValidateSslCertificates = false,
            ActiveEnvironmentId = "env-1"
        });

        var reloaded = await service.GetSettingsAsync();
        reloaded.DarkMode.Should().BeFalse();
        reloaded.DefaultTimeoutSeconds.Should().Be(60);
        reloaded.MaxHistoryItems.Should().Be(500);
        reloaded.AutoFormatJson.Should().BeFalse();
        reloaded.FollowRedirects.Should().BeFalse();
        reloaded.ValidateSslCertificates.Should().BeFalse();
        reloaded.ActiveEnvironmentId.Should().Be("env-1");
    }

    [Fact]
    public async Task UserSettings_ToggleDarkMode_PersistsState()
    {
        var service = new SettingsService(_db);

        var initial = (await service.GetSettingsAsync()).DarkMode;
        await service.ToggleDarkModeAsync();

        var after = (await service.GetSettingsAsync()).DarkMode;
        after.Should().Be(!initial);

        await service.ToggleDarkModeAsync();
        var afterSecond = (await service.GetSettingsAsync()).DarkMode;
        afterSecond.Should().Be(initial);
    }

    [Fact]
    public async Task HistoryItem_TrimOldestWhenOverMax()
    {
        var service = new HistoryService(_db);
        service.SetMaxItems(3);

        var response = new HttpResponseModel { StatusCode = 200 };
        for (int i = 1; i <= 5; i++)
        {
            await service.AddToHistoryAsync(
                new HttpRequestModel { Url = $"https://example.com/{i}" },
                response);
            await Task.Delay(10);
        }

        var history = await service.GetHistoryAsync(10);
        history.Should().HaveCount(3);

        // The 3 most recent should remain (3, 4, 5)
        history.Select(h => h.Url).Should().NotContain("https://example.com/1");
        history.Select(h => h.Url).Should().NotContain("https://example.com/2");
    }

    [Fact]
    public async Task HistoryItem_ClearRemovesAll()
    {
        var service = new HistoryService(_db);
        var response = new HttpResponseModel { StatusCode = 200 };

        await service.AddToHistoryAsync(new HttpRequestModel { Url = "https://1.com" }, response);
        await service.AddToHistoryAsync(new HttpRequestModel { Url = "https://2.com" }, response);

        await service.ClearHistoryAsync();

        var history = await service.GetHistoryAsync();
        history.Should().BeEmpty();

        // Verify the table is actually empty
        var count = await _db.HistoryItems.CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task HistoryItem_SearchByMethod()
    {
        var service = new HistoryService(_db);
        var response = new HttpResponseModel { StatusCode = 200 };

        await service.AddToHistoryAsync(new HttpRequestModel { Method = "GET", Url = "https://a.com" }, response);
        await service.AddToHistoryAsync(new HttpRequestModel { Method = "POST", Url = "https://b.com" }, response);
        await service.AddToHistoryAsync(new HttpRequestModel { Method = "DELETE", Url = "https://c.com" }, response);

        var results = await service.SearchHistoryAsync("POST");
        results.Should().HaveCount(1);
        results[0].Method.Should().Be("POST");
    }
}
