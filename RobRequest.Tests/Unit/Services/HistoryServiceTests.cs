using RobRequest.Shared.Models.Requests;

namespace RobRequest.Tests.Unit.Services;

public class HistoryServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HistoryService _sut;

    public HistoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        _db = new AppDbContext(options);
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();
        TestHelpers.SeedTestUser(_db);
        _sut = new HistoryService(_db, TestHelpers.CreateTestSettingsService(_db), TestHelpers.CreateTestCurrentUser());
    }

    public void Dispose()
    {
        _db.Database.CloseConnection();
        _db.Dispose();
    }

    [Fact]
    public async Task GetHistoryAsync_ReturnsEmptyByDefault()
    {
        var history = await _sut.GetHistoryAsync();

        history.Should().BeEmpty();
    }

    [Fact]
    public async Task AddToHistoryAsync_AddsItem()
    {
        var request = new HttpRequestModel { Method = "GET", Url = "https://example.com" };
        var response = new HttpResponseModel { StatusCode = 200, StatusText = "OK" };

        await _sut.AddToHistoryAsync(request, response);
        var history = await _sut.GetHistoryAsync();

        history.Should().HaveCount(1);
        history[0].Method.Should().Be("GET");
        history[0].Url.Should().Be("https://example.com");
        history[0].StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task AddToHistoryAsync_MostRecentFirst()
    {
        var request1 = new HttpRequestModel { Method = "GET", Url = "https://first.com" };
        var request2 = new HttpRequestModel { Method = "POST", Url = "https://second.com" };
        var response = new HttpResponseModel { StatusCode = 200 };

        await _sut.AddToHistoryAsync(request1, response);
        await Task.Delay(10);
        await _sut.AddToHistoryAsync(request2, response);

        var history = await _sut.GetHistoryAsync();

        history[0].Method.Should().Be("POST");
        history[1].Method.Should().Be("GET");
    }

    [Fact]
    public async Task ClearHistoryAsync_RemovesAllItems()
    {
        var request = new HttpRequestModel { Method = "GET", Url = "https://example.com" };
        var response = new HttpResponseModel { StatusCode = 200 };

        await _sut.AddToHistoryAsync(request, response);
        await _sut.ClearHistoryAsync();

        var history = await _sut.GetHistoryAsync();
        history.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchHistoryAsync_FiltersByUrl()
    {
        var request1 = new HttpRequestModel { Method = "GET", Url = "https://api.example.com/users" };
        var request2 = new HttpRequestModel { Method = "POST", Url = "https://api.example.com/posts" };
        var response = new HttpResponseModel { StatusCode = 200 };

        await _sut.AddToHistoryAsync(request1, response);
        await _sut.AddToHistoryAsync(request2, response);

        var results = await _sut.SearchHistoryAsync("users");

        results.Should().HaveCount(1);
        results[0].Url.Should().Contain("users");
    }

    [Fact]
    public async Task AddToHistoryAsync_TrimsHistoryToMaxItems()
    {
        var sut = new HistoryService(_db, TestHelpers.CreateTestSettingsService(_db, maxHistoryItems: 2), TestHelpers.CreateTestCurrentUser());

        var response = new HttpResponseModel { StatusCode = 200 };
        await sut.AddToHistoryAsync(new HttpRequestModel { Url = "https://1.com" }, response);
        await sut.AddToHistoryAsync(new HttpRequestModel { Url = "https://2.com" }, response);
        await sut.AddToHistoryAsync(new HttpRequestModel { Url = "https://3.com" }, response);

        var history = await sut.GetHistoryAsync();
        history.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetHistoryAsync_RespectsLimitAndOffset()
    {
        var response = new HttpResponseModel { StatusCode = 200 };
        for (var i = 0; i < 25; i++)
        {
            await _sut.AddToHistoryAsync(new HttpRequestModel { Method = "GET", Url = $"https://example.com/{i}" }, response);
            await Task.Delay(2);
        }

        var firstPage = await _sut.GetHistoryAsync(limit: 10, offset: 0);
        var secondPage = await _sut.GetHistoryAsync(limit: 10, offset: 10);
        var thirdPage = await _sut.GetHistoryAsync(limit: 10, offset: 20);

        firstPage.Should().HaveCount(10);
        secondPage.Should().HaveCount(10);
        thirdPage.Should().HaveCount(5);

        // Pages should not overlap (most-recent first, no duplicates across pages).
        var allIds = firstPage.Concat(secondPage).Concat(thirdPage).Select(h => h.Id).ToList();
        allIds.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task SearchHistoryAsync_RespectsLimitAndOffset()
    {
        var response = new HttpResponseModel { StatusCode = 200 };
        for (var i = 0; i < 15; i++)
        {
            await _sut.AddToHistoryAsync(new HttpRequestModel { Method = "GET", Url = $"https://api.example.com/users/{i}" }, response);
            await Task.Delay(2);
        }

        var firstPage = await _sut.SearchHistoryAsync("users", limit: 10, offset: 0);
        var secondPage = await _sut.SearchHistoryAsync("users", limit: 10, offset: 10);

        firstPage.Should().HaveCount(10);
        secondPage.Should().HaveCount(5);
        firstPage.Concat(secondPage).Select(h => h.Id).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task OnHistoryChanged_FiresWhenItemAdded()
    {
        var fired = false;
        _sut.OnHistoryChanged += () => fired = true;

        await _sut.AddToHistoryAsync(
            new HttpRequestModel { Url = "https://example.com" },
            new HttpResponseModel { StatusCode = 200 });

        fired.Should().BeTrue();
    }
}