using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PharmaTrack.PWA;
using PharmaTrack.PWA.Helpers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var authUrl = builder.Configuration["ApiUrls:Auth"] ?? throw new InvalidOperationException("Missing configuration value 'ApiUrls:Auth' (under ApiUrls in appsettings.json).");
var scheduleUrl = builder.Configuration["ApiUrls:Schedule"] ?? throw new InvalidOperationException("Missing configuration value 'ApiUrls:Schedule' (under ApiUrls in appsettings.json).");

// 1) localStorage + auth core
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services
    .AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();

// 2) your JWT handler
builder.Services.AddTransient<JwtAuthorizationHandler>();

// 3) typed HttpClient
builder.Services
  .AddHttpClient<AuthService>(client =>
    client.BaseAddress = new Uri(authUrl));

builder.Services
  .AddHttpClient<ScheduleService>(client =>
    client.BaseAddress = new Uri(scheduleUrl))
  .AddHttpMessageHandler<JwtAuthorizationHandler>();

// 4) (repeat for each back-end)
// builder.Services
//   .AddHttpClient<DrugService>(c => c.BaseAddress = new Uri(builder.Configuration["ApiUrls:Drug"]))
//   .AddHttpMessageHandler<JwtAuthorizationHandler>();

await builder.Build().RunAsync();
