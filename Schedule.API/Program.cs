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

builder.Services.AddDbContext<ScheduleDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

//app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
