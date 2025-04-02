using InventoryScannerCore;
using InventoryScannerCore.Lookups;
using InventoryScannerCore.Repositories;
using InventoryScannerCore.UnitTests;
using InventoryScannerCore.Workflows;
using InventoryScannerCore.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Services.Configure<Settings>(options => builder.Configuration.GetSection("Settings").Bind(options));

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services
    .AddSilverback()
    .WithConnectionToMessageBroker(options => options
        .AddRabbit())
    .AddEndpointsConfigurator<RabbitEndpointsConfigurator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<ISettingsService, SettingsService>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<IInventoryWorkflow, InventoryWorkflow>();
builder.Services.AddScoped<IBarcodeLookup, BarcodeLookup>();
builder.Services.AddScoped<IImageLookup, ImageLookup>();
builder.Services.AddScoped<HttpClient, HttpClient>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
