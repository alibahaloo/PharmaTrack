var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();    // serve index.html by default
app.UseStaticFiles();     // serve wwwroot/**/*

// SPA fallback
app.MapFallbackToFile("index.html");

app.Run();
