using Microsoft.AspNetCore.Components;
using RobRequest.Shared.Services;

namespace RobRequest.Server.Components.Layout.Bases;

public class MainLayoutBase : LayoutComponentBase, IDisposable
{
    [Inject] public required SettingsService SettingsService { get; set; }
    [Inject] public required CurrentUserService CurrentUser { get; set; }

    public bool IsDrawerOpen { get; protected set; } = true;
    public bool IsDarkMode { get; protected set; } = true;
    protected bool IsDrawerEnabled { get; private set; }

    #region UI
    
    protected void DrawerToggle()
    {
        IsDrawerOpen = !IsDrawerOpen;
    }

    public async Task DrawerEnabledToggleAsync(bool enabled)
    {
        IsDrawerEnabled = enabled;
        await InvokeAsync(StateHasChanged);
    }

    protected async Task ToggleDarkMode()
    {
        var settings = await SettingsService.GetSettingsAsync();
        settings.DarkMode = !settings.DarkMode;
        await SettingsService.UpdateSettingsAsync(settings);
        await InvokeAsync(StateHasChanged);
    }
    
    #endregion
    
    #region Settings
    
    private async void OnSettingsChanged()
    {
        try
        {
            var settings = await SettingsService.GetSettingsAsync();
            IsDarkMode = settings.DarkMode;
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception)
        {
            // ignored
        }
    }
    
    #endregion
    
    #region Overrides
    
    protected override async Task OnInitializedAsync()
    {
        var settings = await SettingsService.GetSettingsAsync();
        IsDarkMode = settings.DarkMode;
        SettingsService.OnSettingsChanged += OnSettingsChanged;

        await Task.CompletedTask;
    }

    public void Dispose()
    {
        SettingsService.OnSettingsChanged -= OnSettingsChanged;
    }
    
    #endregion
}