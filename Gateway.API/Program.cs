using Microsoft.AspNetCore.Server.Kestrel.Core;
using PharmaTrack.Shared.Services;

var options = new WebApplicationOptions
{
    Args = args,
    // point the content‐root at the folder where your exe lives
    ContentRootPath = AppContext.BaseDirectory
};

var builder = WebApplication.CreateBuilder(options);
// let it know it’s running as a Windows Service
builder.Host.UseWindowsService(options => {
    // optional: give the service a friendly name in the SCM
    options.ServiceName = "PharmaTrack Gateway API";
});

// if we're running this in production (as a service), then we will read the cert
if (builder.Environment.IsProduction())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        // HTTP endpoint
        options.ListenAnyIP(8081, listenOpts =>
            listenOpts.Protocols = HttpProtocols.Http1AndHttp2);

        // HTTPS endpoint (load your PFX)
        options.ListenAnyIP(8082, listenOpts =>
        {
            listenOpts.UseHttps(
                "certs/PharmaTrackCert.pfx",
                "YourP@ssw0rd!"
            );
        });
    });
}

// Load the shared configuration
var sharedConfiguration = SharedConfiguration.GetSharedConfiguration();
builder.Configuration.AddConfiguration(sharedConfiguration);

builder.Services.AddControllers();

// Register HttpClientFactory
builder.Services.AddHttpClient();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add JWT configuration using the shared extension
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add HttpContextAccessor and Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<JwtService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (string.IsNullOrEmpty(app.Configuration["InventoryApi:BaseUrl"]))
{
    throw new InvalidOperationException("InventoryApi:BaseUrl is not configured. Please check appsettings.json.");
}


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
