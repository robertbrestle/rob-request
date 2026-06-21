namespace RobRequest.Tests.Unit.Services;

public class SettingsServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly SettingsService _sut;

    public SettingsServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        _db = new AppDbContext(options);
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();
        TestHelpers.SeedTestUser(_db);
        _sut = new SettingsService(_db, TestHelpers.CreateTestCurrentUser());
    }

    public void Dispose()
    {
        _db.Database.CloseConnection();
        _db.Dispose();
    }

    [Fact]
    public async Task GetSettingsAsync_ReturnsDefaults()
    {
        var settings = await _sut.GetSettingsAsync();

        settings.DarkMode.Should().BeTrue();
        settings.DefaultTimeoutSeconds.Should().Be(30);
        settings.MaxHistoryItems.Should().Be(1000);
        settings.AutoFormatJson.Should().BeTrue();
        settings.FollowRedirects.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateSettingsAsync_PersistsChanges()
    {
        var settings = await _sut.GetSettingsAsync();
        settings.DarkMode = false;
        settings.DefaultTimeoutSeconds = 60;

        await _sut.UpdateSettingsAsync(settings);

        var updated = await _sut.GetSettingsAsync();
        updated.DarkMode.Should().BeFalse();
        updated.DefaultTimeoutSeconds.Should().Be(60);
    }

    [Fact]
    public async Task ToggleDarkModeAsync_TogglesDarkMode()
    {
        var initial = (await _sut.GetSettingsAsync()).DarkMode;

        await _sut.ToggleDarkModeAsync();

        var toggled = (await _sut.GetSettingsAsync()).DarkMode;
        toggled.Should().Be(!initial);
    }

    [Fact]
    public async Task OnSettingsChanged_FiresOnUpdate()
    {
        var fired = false;
        _sut.OnSettingsChanged += () => fired = true;

        await _sut.UpdateSettingsAsync(new UserSettings { DarkMode = false });

        fired.Should().BeTrue();
    }
}