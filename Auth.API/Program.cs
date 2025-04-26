using Auth.API.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using PharmaTrack.Shared.DBModels;
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
    options.ServiceName = "PharmaTrack Auth API";
});

// if we're running this in production (as a service), then we will read the cert
if (builder.Environment.IsProduction())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        // HTTP endpoint
        options.ListenAnyIP(8083, listenOpts =>
            listenOpts.Protocols = HttpProtocols.Http1AndHttp2);

        // HTTPS endpoint (load your PFX)
        options.ListenAnyIP(8084, listenOpts =>
        {
            listenOpts.UseHttps(
                "certs/PharmaTrackCert.pfx",
                "YourP@ssw0rd!"
            );
        });
    });
}
/*
builder.WebHost.UseUrls(
    "http://localhost:8083",
    "https://localhost:8084"
);*/

// Load the shared configuration
var sharedConfiguration = SharedConfiguration.GetSharedConfiguration();
builder.Configuration.AddConfiguration(sharedConfiguration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AuthDBContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthDBContext>()
    .AddDefaultTokenProviders();

// Add JWT configuration using the shared extension
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add HttpContextAccessor and Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<JwtService>();

var app = builder.Build();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDBContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
