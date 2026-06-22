using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using RobRequest.Server.Components.Layout.Bases;

namespace RobRequest.Tests.Unit.Components;

public class MainLayoutThemeTests
{
    /// <summary>
    /// Test-only subclass exposing the protected dark-mode toggle so the
    /// component logic can be exercised without the full MudBlazor render tree.
    /// </summary>
    private sealed class TestableMainLayout : MainLayoutBase
    {
        public Task InvokeToggleDarkModeAsync() => ToggleDarkMode();
    }

    private static (BunitContext ctx, AppDbContext db) CreateContext()
    {
        var ctx = new BunitContext();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        var db = new AppDbContext(options);
        db.Database.OpenConnection();
        db.Database.EnsureCreated();
        TestHelpers.SeedTestUser(db);

        ctx.Services.AddSingleton(db);
        ctx.Services.AddSingleton(TestHelpers.CreateTestCurrentUser());
        ctx.Services.AddScoped<SettingsService>();
        ctx.Services.AddSingleton(new Mock<IDialogService>().Object);

        return (ctx, db);
    }

    [Fact]
    public async Task ToggleDarkMode_AfterLayoutDisposed_StillUpdatesIsDarkMode()
    {
        var (ctx, db) = CreateContext();
        try
        {
            var cut = ctx.Render<TestableMainLayout>();
            var layout = cut.Instance;

            layout.IsDarkMode.Should().BeTrue("the seeded user starts in dark mode");

            // Simulate the layout being torn down on navigation, which unsubscribes
            // it from SettingsService.OnSettingsChanged.
            layout.Dispose();

            var before = layout.IsDarkMode;
            await cut.InvokeAsync(() => layout.InvokeToggleDarkModeAsync());

            layout.IsDarkMode.Should().Be(!before,
                "toggling the theme must update the local state even when the settings event is no longer subscribed");
        }
        finally
        {
            db.Database.CloseConnection();
            db.Dispose();
            ctx.Dispose();
        }
    }

    [Fact]
    public async Task ToggleDarkMode_PersistsNewValueToSettings()
    {
        var (ctx, db) = CreateContext();
        try
        {
            var cut = ctx.Render<TestableMainLayout>();
            var layout = cut.Instance;
            var settingsService = ctx.Services.GetRequiredService<SettingsService>();

            var before = (await settingsService.GetSettingsAsync()).DarkMode;
            await cut.InvokeAsync(() => layout.InvokeToggleDarkModeAsync());

            layout.IsDarkMode.Should().Be(!before);
            (await settingsService.GetSettingsAsync()).DarkMode.Should().Be(!before);
        }
        finally
        {
            db.Database.CloseConnection();
            db.Dispose();
            ctx.Dispose();
        }
    }
}
