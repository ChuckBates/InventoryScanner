using InventoryScanner.Core.Lookups;
using InventoryScanner.Core.Repositories;
using InventoryScanner.Core.UnitTests;
using InventoryScanner.Core.Workflows;
using InventoryScanner.Core.Settings;
using EasyNetQ;
using InventoryScanner.Messaging.Interfaces;
using Microsoft.Extensions.Options;
using InventoryScanner.Messaging.Publishing;
using InventoryScanner.Core.Publishers;
using EasyNetQ.Serialization.SystemTextJson;
using EasyNetQ.DI;
using InventoryScanner.Messaging.Infrastructure;
using InventoryScanner.Messaging.Subscribing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Services.Configure<Settings>(options => builder.Configuration.GetSection("Settings").Bind(options));

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

var rabbitSettings = builder.Configuration.GetSection("Settings:RabbitMQ").Get<RabbitMqSettings>();
var connectionString = $"host={rabbitSettings?.HostName}:{rabbitSettings?.AmqpPort};username={rabbitSettings?.UserName};password={rabbitSettings?.Password}";

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<ISettingsService, SettingsService>();
builder.Services.AddSingleton<IRabbitMqSettings>(sp => sp.GetRequiredService<IOptions<RabbitMqSettings>>().Value);

builder.Services.Configure<List<RabbitMqInfrastructureTarget>>(opts =>
{
    opts.Add(new RabbitMqInfrastructureTarget
    {
        ExchangeName = rabbitSettings?.FetchInventoryMetadataExchangeName ?? string.Empty,
        QueueName = rabbitSettings?.FetchInventoryMetadataQueueName ?? string.Empty,
        ExchangeType = "fanout"
    });
    opts.Add(new RabbitMqInfrastructureTarget
    {
        ExchangeName = rabbitSettings?.FetchInventoryMetadataDeadLetterExchangeName ?? string.Empty,
        QueueName = rabbitSettings?.FetchInventoryMetadataDeadLetterQueueName ?? string.Empty,
        ExchangeType = "fanout"
    });
});

builder.Services.AddMessaging(connectionString, startup: true);
builder.Services.AddSingleton<IRabbitMqSubscriberLifecycleObserver, EmptyRabbitMqLifecycleObserver>();

builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<IFetchInventoryMetadataRequestPublisher, FetchInventoryMetadataRequestPublisher>();
builder.Services.AddScoped<IInventoryWorkflow, InventoryWorkflow>();
builder.Services.AddScoped<IBarcodeLookup, BarcodeLookup>();
builder.Services.AddScoped<IImageLookup, ImageLookup>();
builder.Services.AddScoped<HttpClient, HttpClient>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();
