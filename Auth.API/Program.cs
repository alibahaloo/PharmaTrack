using Auth.API.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PharmaTrack.Shared.DBModels;
using PharmaTrack.Shared.Services;

var builder = WebApplication.CreateBuilder(args);

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
