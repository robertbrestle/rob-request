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
        _sut = new HistoryService(_db, TestHelpers.CreateTestCurrentUser());
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
    public async Task SetMaxItems_TrimsHistoryOnAdd()
    {
        _sut.SetMaxItems(2);

        var response = new HttpResponseModel { StatusCode = 200 };
        await _sut.AddToHistoryAsync(new HttpRequestModel { Url = "https://1.com" }, response);
        await _sut.AddToHistoryAsync(new HttpRequestModel { Url = "https://2.com" }, response);
        await _sut.AddToHistoryAsync(new HttpRequestModel { Url = "https://3.com" }, response);

        var history = await _sut.GetHistoryAsync();
        history.Should().HaveCount(2);
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