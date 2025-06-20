var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// 1) Enable serving of Blazor’s built-in framework assets  
app.UseBlazorFrameworkFiles();

// 2) Serve any other static files in wwwroot  
app.UseStaticFiles();

// 3) Serve index.html by default  
app.UseDefaultFiles();

// 4) Optional: fallback all non-file requests to index.html for client-side routing  
app.MapFallbackToFile("index.html");

app.Run();
