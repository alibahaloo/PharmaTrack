using BlazorBootstrap;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PharmaTrack.PWA;
using PharmaTrack.PWA.Helpers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var authUrl = builder.Configuration["ApiUrls:Auth"] ?? throw new InvalidOperationException("Missing configuration value 'ApiUrls:Auth' (under ApiUrls in appsettings.json).");
var scheduleUrl = builder.Configuration["ApiUrls:Schedule"] ?? throw new InvalidOperationException("Missing configuration value 'ApiUrls:Schedule' (under ApiUrls in appsettings.json).");
var drugUrl = builder.Configuration["ApiUrls:Drug"] ?? throw new InvalidOperationException("Missing configuration value 'ApiUrls:Drug' (under ApiUrls in appsettings.json).");
var inventoryUrl = builder.Configuration["ApiUrls:Inventory"] ?? throw new InvalidOperationException("Missing configuration value 'ApiUrls:Inventory' (under ApiUrls in appsettings.json).");

builder.Services.AddBlazorBootstrap();

// 1) localStorage + auth core
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services
    .AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();

// 2) your JWT handler
builder.Services.AddTransient<JwtRefreshHandler>();

// 3) typed HttpClient
builder.Services
  .AddHttpClient<AuthService>(client =>
    client.BaseAddress = new Uri(authUrl));

builder.Services
  .AddHttpClient<ScheduleService>(client =>
    client.BaseAddress = new Uri(scheduleUrl))
  .AddHttpMessageHandler<JwtRefreshHandler>();

builder.Services
  .AddHttpClient<DrugService>(client =>
    client.BaseAddress = new Uri(drugUrl))
  .AddHttpMessageHandler<JwtRefreshHandler>();

builder.Services
  .AddHttpClient<InventoryService>(client =>
    client.BaseAddress = new Uri(inventoryUrl))
  .AddHttpMessageHandler<JwtRefreshHandler>();

builder.Services.AddSingleton<ToastHostService>();

await builder.Build().RunAsync();
