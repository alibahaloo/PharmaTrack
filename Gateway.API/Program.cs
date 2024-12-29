var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Register HttpClientFactory
builder.Services.AddHttpClient();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
