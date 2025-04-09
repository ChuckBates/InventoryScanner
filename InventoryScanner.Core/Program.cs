using InventoryScanner.Core.Lookups;
using InventoryScanner.Core.Repositories;
using InventoryScanner.Core.UnitTests;
using InventoryScanner.Core.Workflows;
using InventoryScanner.Core.Settings;
using InventoryScanner.Messaging.Interfaces;
using InventoryScanner.Core.Publishers;
using InventoryScanner.Messaging.Infrastructure;
using InventoryScanner.Core.Observers;
using InventoryScanner.Core.Subscribers;
using Microsoft.Extensions.Options;
using InventoryScanner.Core.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

var settingsSection = builder.Configuration.GetSection("Settings");
var rabbitMqSettings = settingsSection.Get<Settings>()?.RabbitMQ ?? throw new InvalidOperationException("Rabbit settings are invalid.");

builder.Services.Configure<Settings>(settingsSection);
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<Settings>>().Value);
builder.Services.AddSingleton<ISettingsService, SettingsService>();
builder.Services.AddSingleton<IRabbitMqSettings>(rabbitMqSettings ?? throw new InvalidOperationException("RabbitMQ settings are missing or incomplete."));

builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<List<RabbitMqInfrastructureTarget>>(opts =>
{
    opts.Add(new RabbitMqInfrastructureTarget
    {
        ExchangeName = rabbitMqSettings.FetchInventoryMetadataExchangeName ?? string.Empty,
        QueueName = rabbitMqSettings.FetchInventoryMetadataQueueName ?? string.Empty,
        ExchangeType = "fanout"
    });
    opts.Add(new RabbitMqInfrastructureTarget
    {
        ExchangeName = rabbitMqSettings.FetchInventoryMetadataDeadLetterExchangeName ?? string.Empty,
        QueueName = rabbitMqSettings.FetchInventoryMetadataDeadLetterQueueName ?? string.Empty,
        ExchangeType = "fanout"
    });
});

var connectionString = $"host={rabbitMqSettings.HostName}:{rabbitMqSettings.AmqpPort};username={rabbitMqSettings.UserName};password={rabbitMqSettings.Password}";
builder.Services.AddMessaging(connectionString, startup: true);
builder.Services.AddSingleton<IRabbitMqSubscriberLifecycleObserver, FetchInventoryMetadataObserver>();
builder.Services.AddHostedService<FetchInventoryMetadataSubscriber>();

builder.Services.AddSingleton<IInventoryRepository, InventoryRepository>();
builder.Services.AddSingleton<IImageRepository, ImageRepository>();
builder.Services.AddSingleton<IFetchInventoryMetadataRequestPublisher, FetchInventoryMetadataRequestPublisher>();
builder.Services.AddSingleton<IFetchInventoryMetadataRequestDeadLetterPublisher, FetchInventoryMetadataRequestDeadLetterPublisher>();
builder.Services.AddSingleton<IFetchInventoryMetadataMessageHandler, FetchInventoryMetadataMessageHandler>();
builder.Services.AddScoped<IInventoryWorkflow, InventoryWorkflow>();
builder.Services.AddSingleton<IBarcodeLookup, BarcodeLookup>();
builder.Services.AddSingleton<IImageLookup, ImageLookup>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();
