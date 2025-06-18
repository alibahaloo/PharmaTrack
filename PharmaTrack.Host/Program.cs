using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// 1) Default document (index.html)
app.UseDefaultFiles();

// 2) Serve ALL static files, including .dat
var provider = new FileExtensionContentTypeProvider();
// if you just want to whitelist .dat, you can do:
// provider.Mappings[".dat"] = "application/octet-stream";

app.UseStaticFiles(new StaticFileOptions
{
    // allow unknown extensions (i.e. .dat)
    ServeUnknownFileTypes = true,
    // fallback MIME type
    DefaultContentType = "application/octet-stream",
    // still use built-in mappings for known types
    ContentTypeProvider = provider
});

// 3) SPA fallback
app.MapFallbackToFile("index.html");

app.Run();
