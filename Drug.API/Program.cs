using Drug.API.Data;
using Drug.API.Services;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
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
    options.ServiceName = "PharmaTrack Drug API";
});

// Configure URL and ports
builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.Configure(context.Configuration.GetSection("Kestrel"));
});

var blazorURL = builder.Configuration["Cors:BlazorClient"] ?? throw new InvalidOperationException("Cors:BlazorClient not found in appsettings.json");

// Add CORS support
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy
        .WithOrigins(blazorURL)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<DrugDBContext>(options =>
    options.UseSqlServer(connectionString));

// Add Hangfire services
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseDefaultTypeSerializer()
    .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
    {
        PrepareSchemaIfNecessary = true,
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.FromSeconds(15),
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

// Add Hangfire server
builder.Services.AddHangfireServer();
builder.Services.AddHealthChecks();

builder.Services.AddScoped<DrugJobService>();

// Load the shared configuration
var sharedConfiguration = SharedConfiguration.GetSharedConfiguration();
builder.Configuration.AddConfiguration(sharedConfiguration);

// Add JWT configuration using the shared extension
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("AllowBlazorClient");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Use Hangfire dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AllowAllDashboardAuthorizationFilter() }
});

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

app.Run();

public class AllowAllDashboardAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true; // Allows all access
    }
}