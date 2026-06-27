using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using MudExtensions.Services;
using RobRequest.Shared.Data;
using RobRequest.Shared.Models;
using RobRequest.Shared.Services;
using RobRequest.Server.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.VisibleStateDuration = 3000;
    config.SnackbarConfiguration.HideTransitionDuration = 300;
    config.SnackbarConfiguration.ShowTransitionDuration = 300;
});
builder.Services.AddMudExtensions();

// Authentication & Authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy => policy.RequireClaim("group", "admin"));
builder.Services.AddCascadingAuthenticationState();

// Register EF Core with SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Data Source=robrequest.db"));

// Password hasher
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

// Register services with Scoped lifetime (one instance per SignalR circuit)
builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddHttpClient("Default")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        AutomaticDecompression = DecompressionMethods.All
    });
builder.Services.AddHttpClient("NoSslValidation")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
        AutomaticDecompression = DecompressionMethods.All
    });
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<HistoryService>();
builder.Services.AddScoped<CollectionService>();
builder.Services.AddScoped<EnvironmentService>();
builder.Services.AddScoped<SettingsService>();
builder.Services.AddScoped<ImportExportService>();

var app = builder.Build();

// Auto-migrate database and seed admin user on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    // vacuum the database automatically
    await db.Database.ExecuteSqlRawAsync("PRAGMA auto_vacuum = INCREMENTAL;");
    await db.Database.ExecuteSqlRawAsync("PRAGMA incremental_vacuum;");

    // Seed user groups
    if (!await db.UserGroups.AnyAsync())
    {
        db.UserGroups.AddRange(
            new UserGroup { Name = "admin" },
            new UserGroup { Name = "user" });
        await db.SaveChangesAsync();
    }

    // Seed admin user if no admin exists
    var adminGroup = await db.UserGroups.FirstAsync(g => g.Name == "admin");
    if (!await db.Users.AnyAsync(u => u.GroupId == adminGroup.Id))
    {
        var password = Guid.NewGuid().ToString();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var admin = new User
        {
            Username = "admin",
            PasswordHash = hasher.HashPassword(null!, password),
            GroupId = adminGroup.Id,
            IsEnabled = true,
            IsApproved = true
        };
        db.Users.Add(admin);

        // Create default settings for admin
        db.UserSettings.Add(new UserSettings { UserId = admin.Id });
        await db.SaveChangesAsync();

        Console.WriteLine("========================================");
        Console.WriteLine("  Admin account created");
        Console.WriteLine($"  Username: admin");
        Console.WriteLine($"  Password: {password}");
        Console.WriteLine("========================================");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
else
{
    // Only force HTTPS redirect in Development
    app.UseHttpsRedirection();
}

// allows for proper redirection to NotFound.razor
// https://github.com/dotnet/aspnetcore/issues/62404
app.UseStatusCodePagesWithReExecute("/not-found", "?statusCode={0}");

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// Auth API endpoints (cookie auth requires HttpContext, which is not available in SignalR components)
app.MapPost("/api/auth/login", async (HttpContext ctx, AuthService authService) =>
{
    var form = await ctx.Request.ReadFormAsync();
    var username = form["username"].ToString();
    var password = form["password"].ToString();

    var user = await authService.LoginAsync(username, password);
    if (user == null)
    {
        ctx.Response.Redirect("/login?error=invalid");
        return;
    }

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id),
        new(ClaimTypes.Name, user.Username),
        new("group", user.Group?.Name ?? "user")
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
        new AuthenticationProperties { IsPersistent = true });

    ctx.Response.Redirect("/");
});

app.MapPost("/api/auth/register", async (HttpContext ctx, AuthService authService) =>
{
    var form = await ctx.Request.ReadFormAsync();
    var username = form["username"].ToString();
    var password = form["password"].ToString();
    var confirmPassword = form["confirmPassword"].ToString();

    if (password != confirmPassword)
    {
        ctx.Response.Redirect($"/register?error={Uri.EscapeDataString("Passwords do not match.")}");
        return;
    }

    var (success, error) = await authService.RegisterAsync(username, password);
    if (!success)
    {
        ctx.Response.Redirect($"/register?error={Uri.EscapeDataString(error ?? "Registration failed.")}");
        return;
    }

    ctx.Response.Redirect("/login?registered=true");
});

app.MapGet("/api/auth/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    ctx.Response.Redirect("/login");
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();