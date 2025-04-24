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
using Serilog;
using Serilog.Events;
using InventoryScanner.Core.Publishers.Interfaces;
using Serilog.Formatting.Compact;
using InventoryScanner.Logging;
using Serilog.Formatting.Json;
using InventoryScanner.Core.Wrappers;

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
    opts.Add(new RabbitMqInfrastructureTarget
    {
        ExchangeName = rabbitMqSettings.InventoryUpdatedExchangeName ?? string.Empty,
        QueueName = rabbitMqSettings.InventoryUpdatedQueueName ?? string.Empty,
        ExchangeType = "fanout"
    });
    opts.Add(new RabbitMqInfrastructureTarget
    {
        ExchangeName = rabbitMqSettings.InventoryUpdatedDeadLetterExchangeName ?? string.Empty,
        QueueName = rabbitMqSettings.InventoryUpdatedDeadLetterQueueName ?? string.Empty,
        ExchangeType = "fanout"
    });
});

var connectionString = $"host={rabbitMqSettings.HostName}:{rabbitMqSettings.AmqpPort};username={rabbitMqSettings.UserName};password={rabbitMqSettings.Password}";
builder.Services.AddMessaging(connectionString, startup: true);

builder.Services.AddSingleton<FetchInventoryMetadataObserver>();
builder.Services.AddSingleton<FetchInventoryMetadataSubscriber>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<FetchInventoryMetadataSubscriber>());

builder.Services.AddSingleton<InventoryUpdatedObserver>();
builder.Services.AddSingleton<InventoryUpdatedSubscriber>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<InventoryUpdatedSubscriber>());

builder.Services.AddSingleton<IInventoryRepository, InventoryRepository>();
builder.Services.AddSingleton<IImageRepository, ImageRepository>();
builder.Services.AddScoped<IInventoryWorkflow, InventoryWorkflow>();
builder.Services.AddSingleton<IBarcodeWrapper, BarcodeWrapper>();
builder.Services.AddSingleton<IImageWrapper, ImageWrapper>();

builder.Services.AddSingleton<IFetchInventoryMetadataRequestPublisher, FetchInventoryMetadataRequestPublisher>();
builder.Services.AddSingleton<IFetchInventoryMetadataRequestDeadLetterPublisher, FetchInventoryMetadataRequestDeadLetterPublisher>();
builder.Services.AddSingleton<IFetchInventoryMetadataMessageHandler, FetchInventoryMetadataMessageHandler>();

builder.Services.AddSingleton<IInventoryUpdatedPublisher, InventoryUpdatedPublisher>();
builder.Services.AddSingleton<IInventoryUpdatedDeadLetterPublisher, InventoryUpdatedDeadLetterPublisher>();
builder.Services.AddSingleton<IInventoryUpdatedMessageHandler, InventoryUpdatedMessageHandler>();

builder.Services.AddSingleton<IInventoryUpdatesWebsocketHandler, InventoryUpdatesWebsocketHandler>();

builder.Services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .MinimumLevel.Override("EasyNetQ", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Server.Kestrel", LogEventLevel.Error)
    .Enrich.FromLogContext()
    .WriteTo.Console(new JsonFormatter(renderMessage: true))
    .WriteTo.File(
        new JsonFormatter(renderMessage: true),
        "logs/inventoryscanner.core.log", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var app = builder.Build();

app.UseAuthorization();
app.UseWebSockets();
app.MapControllers();

app.Run();
