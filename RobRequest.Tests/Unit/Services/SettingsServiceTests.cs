namespace RobRequest.Tests.Unit.Services;

public class SettingsServiceTests
{
    private readonly SettingsService _sut = new();

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
    public void OnSettingsChanged_FiresOnUpdate()
    {
        var fired = false;
        _sut.OnSettingsChanged += () => fired = true;

        _sut.UpdateSettingsAsync(new UserSettings { DarkMode = false });

        fired.Should().BeTrue();
    }
}
