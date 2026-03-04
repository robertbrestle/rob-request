using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using RobRequest.Shared.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();

builder.Services.AddHttpClient<ApiService>();
builder.Services.AddSingleton<HistoryService>();
builder.Services.AddSingleton<EnvironmentService>();
builder.Services.AddSingleton<SettingsService>();

await builder.Build().RunAsync();
