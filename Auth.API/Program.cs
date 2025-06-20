using Auth.API.Data;
using Microsoft.AspNetCore.Identity;
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
    options.ServiceName = "PharmaTrack Auth API";
});

// Load the shared configuration
var sharedConfiguration = SharedConfiguration.GetSharedConfiguration();
builder.Configuration.AddConfiguration(sharedConfiguration);

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

app.UseCors("AllowBlazorClient");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
