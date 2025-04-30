using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using PharmaTrack.Shared.Services;
using Schedule.API.Data;

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
    options.ServiceName = "PharmaTrack Schedule API";
});

// if we're running this in production (as a service), then we will read the cert
if (builder.Environment.IsProduction())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        // HTTP endpoint
        options.ListenAnyIP(8089, listenOpts =>
            listenOpts.Protocols = HttpProtocols.Http1AndHttp2);

        // HTTPS endpoint (load your PFX)
        options.ListenAnyIP(8090, listenOpts =>
        {
            listenOpts.UseHttps(
                "certs/PharmaTrackCert.pfx",
                "YourP@ssw0rd!"
            );
        });
    });
}

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ScheduleDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Load the shared configuration
var sharedConfiguration = SharedConfiguration.GetSharedConfiguration();
builder.Configuration.AddConfiguration(sharedConfiguration);

// Add JWT configuration using the shared extension
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
