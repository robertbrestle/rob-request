using MudBlazor.Services;
using RobRequest.Shared.Services;
using RobRequest.Server.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

// Register services with Scoped lifetime (one instance per SignalR circuit)
builder.Services.AddHttpClient<ApiService>();
builder.Services.AddScoped<HistoryService>();
builder.Services.AddScoped<EnvironmentService>();
builder.Services.AddScoped<SettingsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();