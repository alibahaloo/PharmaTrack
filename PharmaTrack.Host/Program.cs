var exePath = AppContext.BaseDirectory;

var options = new WebApplicationOptions
{
    Args = args,
    ContentRootPath = exePath,
    WebRootPath = Path.Combine(exePath, "wwwroot") 
};

var builder = WebApplication.CreateBuilder(options);

// Enable Windows Service
builder.Host.UseWindowsService(options =>
{
    options.ServiceName = "PharmaTrack Host";
});

var app = builder.Build();

app.UseBlazorFrameworkFiles();     
app.UseStaticFiles();              
app.UseDefaultFiles();
app.MapFallbackToFile("index.html");

app.Run();
