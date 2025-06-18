var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// 1) Serve the client’s wwwroot (including _framework) as Blazor framework files
app.UseBlazorFrameworkFiles();

// 2) Serve any other static files in your wwwroot (css, images, index.html)
app.UseStaticFiles();

// 3) If no other route matches, fall back to index.html so the Blazor router can take over
app.MapFallbackToFile("index.html");

app.Run();
