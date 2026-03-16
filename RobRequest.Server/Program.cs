using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using RobRequest.Shared.Data;
using RobRequest.Shared.Services;
using RobRequest.Server.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

// Register EF Core with SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=robrequest.db"));

// Register services with Scoped lifetime (one instance per SignalR circuit)
builder.Services.AddHttpClient<ApiService>();
builder.Services.AddScoped<HistoryService>();
builder.Services.AddScoped<CollectionService>();
builder.Services.AddScoped<EnvironmentService>();
builder.Services.AddScoped<SettingsService>();
builder.Services.AddScoped<ImportExportService>();

var app = builder.Build();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

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