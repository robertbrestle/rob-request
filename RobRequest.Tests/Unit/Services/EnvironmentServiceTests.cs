namespace RobRequest.Tests.Unit.Services;

public class EnvironmentServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly SettingsService _settingsService;
    private readonly EnvironmentService _sut;
    private readonly CurrentUserService _currentUser;

    public EnvironmentServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        _db = new AppDbContext(options);
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();
        TestHelpers.SeedTestUser(_db);
        _currentUser = TestHelpers.CreateTestCurrentUser();
        _settingsService = new SettingsService(_db, _currentUser);
        _sut = new EnvironmentService(_db, _settingsService, _currentUser);
    }

    public void Dispose()
    {
        _db.Database.CloseConnection();
        _db.Dispose();
    }

    [Fact]
    public async Task GetAllEnvironmentsAsync_ReturnsEmptyByDefault()
    {
        var environments = await _sut.GetAllEnvironmentsAsync();

        environments.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateEnvironmentAsync_CreatesAndReturnsEnvironment()
    {
        var env = await _sut.CreateEnvironmentAsync("Development");

        env.Should().NotBeNull();
        env.Name.Should().Be("Development");
        env.Id.Should().NotBeNullOrEmpty();

        var all = await _sut.GetAllEnvironmentsAsync();
        all.Should().HaveCount(1);
        all[0].Name.Should().Be("Development");
    }

    [Fact]
    public async Task CreateEnvironmentAsync_AssignsIncrementingSortOrder()
    {
        await _sut.CreateEnvironmentAsync("First");
        await _sut.CreateEnvironmentAsync("Second");
        await _sut.CreateEnvironmentAsync("Third");

        var all = await _sut.GetAllEnvironmentsAsync();
        all.Should().HaveCount(3);
        all[0].SortOrder.Should().Be(0);
        all[1].SortOrder.Should().Be(1);
        all[2].SortOrder.Should().Be(2);
    }

    [Fact]
    public async Task GetEnvironmentAsync_ReturnsCorrectEnvironment()
    {
        var created = await _sut.CreateEnvironmentAsync("Test");

        var fetched = await _sut.GetEnvironmentAsync(created.Id);

        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetEnvironmentAsync_ReturnsNullForUnknownId()
    {
        var result = await _sut.GetEnvironmentAsync("nonexistent");

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateEnvironmentAsync_PersistsChanges()
    {
        var env = await _sut.CreateEnvironmentAsync("Original");
        env.Name = "Updated";
        env.Description = "A description";

        await _sut.UpdateEnvironmentAsync(env);

        var fetched = await _sut.GetEnvironmentAsync(env.Id);
        fetched!.Name.Should().Be("Updated");
        fetched.Description.Should().Be("A description");
    }

    [Fact]
    public async Task DeleteEnvironmentAsync_RemovesEnvironment()
    {
        var env = await _sut.CreateEnvironmentAsync("ToDelete");

        await _sut.DeleteEnvironmentAsync(env.Id);

        var all = await _sut.GetAllEnvironmentsAsync();
        all.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteEnvironmentAsync_ClearsActiveIdIfDeleted()
    {
        var env = await _sut.CreateEnvironmentAsync("Active");
        _sut.ActiveEnvironmentId = env.Id;

        await _sut.DeleteEnvironmentAsync(env.Id);

        _sut.ActiveEnvironmentId.Should().BeNull();
    }

    [Fact]
    public async Task DeleteEnvironmentAsync_FallsBackToFirstEnvironment()
    {
        var first = await _sut.CreateEnvironmentAsync("First");
        var second = await _sut.CreateEnvironmentAsync("Second");
        _sut.ActiveEnvironmentId = second.Id;

        await _sut.DeleteEnvironmentAsync(second.Id);

        _sut.ActiveEnvironmentId.Should().Be(first.Id);
    }

    [Fact]
    public async Task SearchEnvironmentsAsync_FindsByName()
    {
        await _sut.CreateEnvironmentAsync("Development");
        await _sut.CreateEnvironmentAsync("Production");
        await _sut.CreateEnvironmentAsync("Staging");

        var results = await _sut.SearchEnvironmentsAsync("prod");

        results.Should().HaveCount(1);
        results[0].Name.Should().Be("Production");
    }

    [Fact]
    public async Task SetVariableAsync_AddsNewVariable()
    {
        var env = await _sut.CreateEnvironmentAsync("Test");

        await _sut.SetVariableAsync(env.Id, "baseUrl", "https://api.example.com");

        var fetched = await _sut.GetEnvironmentAsync(env.Id);
        fetched!.Variables.Should().HaveCount(1);
        fetched.Variables[0].Key.Should().Be("baseUrl");
        fetched.Variables[0].Value.Should().Be("https://api.example.com");
    }

    [Fact]
    public async Task SetVariableAsync_UpdatesExistingVariable()
    {
        var env = await _sut.CreateEnvironmentAsync("Test");
        await _sut.SetVariableAsync(env.Id, "key", "value1");

        await _sut.SetVariableAsync(env.Id, "key", "value2");

        var fetched = await _sut.GetEnvironmentAsync(env.Id);
        fetched!.Variables.Should().ContainSingle(v => v.Key == "key")
            .Which.Value.Should().Be("value2");
    }

    [Fact]
    public async Task SubstituteVariablesAsync_ReplacesVariables()
    {
        var env = await _sut.CreateEnvironmentAsync("Test");
        await _sut.SetVariableAsync(env.Id, "baseUrl", "https://api.example.com");
        _sut.ActiveEnvironmentId = env.Id;

        var result = await _sut.SubstituteVariablesAsync("{{baseUrl}}/users");

        result.Should().Be("https://api.example.com/users");
    }

    [Fact]
    public async Task SubstituteVariablesAsync_LeavesUnknownVariablesIntact()
    {
        var env = await _sut.CreateEnvironmentAsync("Test");
        _sut.ActiveEnvironmentId = env.Id;

        var result = await _sut.SubstituteVariablesAsync("{{unknown}}/path");

        result.Should().Be("{{unknown}}/path");
    }

    [Fact]
    public async Task SubstituteVariablesAsync_ReturnsInputWhenEmpty()
    {
        var result = await _sut.SubstituteVariablesAsync(string.Empty);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SubstituteVariablesAsync_ReturnsInputWhenNoActiveEnvironment()
    {
        var result = await _sut.SubstituteVariablesAsync("{{baseUrl}}/users");

        result.Should().Be("{{baseUrl}}/users");
    }

    [Fact]
    public async Task SubstituteVariablesAsync_SkipsDisabledVariables()
    {
        var env = await _sut.CreateEnvironmentAsync("Test");
        env.Variables.Add(new EnvironmentVariable { Key = "disabled", Value = "nope", Enabled = false });
        env.Variables.Add(new EnvironmentVariable { Key = "enabled", Value = "yes", Enabled = true });
        await _sut.UpdateEnvironmentAsync(env);
        _sut.ActiveEnvironmentId = env.Id;

        var result = await _sut.SubstituteVariablesAsync("{{disabled}} {{enabled}}");

        result.Should().Be("{{disabled}} yes");
    }

    [Fact]
    public void ContainsVariables_ReturnsTrueWhenVariablesPresent()
    {
        EnvironmentService.ContainsVariables("{{baseUrl}}/path").Should().BeTrue();
    }

    [Fact]
    public void ContainsVariables_ReturnsFalseWhenNoVariables()
    {
        EnvironmentService.ContainsVariables("https://example.com").Should().BeFalse();
    }

    [Fact]
    public void ContainsVariables_ReturnsFalseForNullOrEmpty()
    {
        EnvironmentService.ContainsVariables(null).Should().BeFalse();
        EnvironmentService.ContainsVariables(string.Empty).Should().BeFalse();
    }

    [Fact]
    public async Task OnEnvironmentChanged_FiresOnCreate()
    {
        var fired = false;
        _sut.OnEnvironmentChanged += () => fired = true;

        await _sut.CreateEnvironmentAsync("Test");

        fired.Should().BeTrue();
    }

    [Fact]
    public async Task OnEnvironmentChanged_FiresOnDelete()
    {
        var env = await _sut.CreateEnvironmentAsync("Test");
        var fired = false;
        _sut.OnEnvironmentChanged += () => fired = true;

        await _sut.DeleteEnvironmentAsync(env.Id);

        fired.Should().BeTrue();
    }

    [Fact]
    public async Task InitializeAsync_LoadsActiveEnvironmentFromSettings()
    {
        var env = await _sut.CreateEnvironmentAsync("Persisted");
        var settings = await _settingsService.GetSettingsAsync();
        settings.ActiveEnvironmentId = env.Id;
        await _settingsService.UpdateSettingsAsync(settings);

        // Create a fresh service to test initialization
        var freshService = new EnvironmentService(_db, _settingsService, _currentUser);
        await freshService.InitializeAsync();

        freshService.ActiveEnvironmentId.Should().Be(env.Id);
    }
}