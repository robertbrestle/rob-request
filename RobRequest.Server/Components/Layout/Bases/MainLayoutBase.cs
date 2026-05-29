using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
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
    
    [CascadingParameter]
    protected Task<AuthenticationState>? AuthStateTask { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (AuthStateTask != null)
        {
            var authState = await AuthStateTask;
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var username = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
                var groupName = user.FindFirst("group")?.Value;
                CurrentUser.SetUser(userId, username, groupName);
            }
        }

        CurrentUser.OnUserChanged += OnUserChanged;
        var settings = await SettingsService.GetSettingsAsync();
        IsDarkMode = settings.DarkMode;
        SettingsService.OnSettingsChanged += OnSettingsChanged;
    }

    private void OnUserChanged()
    {
        OnSettingsChanged();
    }

    public void Dispose()
    {
        CurrentUser.OnUserChanged -= OnUserChanged;
        SettingsService.OnSettingsChanged -= OnSettingsChanged;
    }
    
    #endregion
}