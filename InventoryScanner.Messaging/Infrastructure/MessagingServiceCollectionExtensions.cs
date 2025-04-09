using EasyNetQ;
using EasyNetQ.DI;
using EasyNetQ.Serialization.SystemTextJson;
using InventoryScanner.Messaging.Interfaces;
using InventoryScanner.Messaging.Publishing;
using InventoryScanner.Messaging.Subscribing;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryScanner.Messaging.Infrastructure
{
    public static class MessagingServiceCollectionExtensions
    {
        public static IServiceCollection AddMessaging(this IServiceCollection services, string connectionString, bool startup = true)
        {
            services.AddSingleton(RabbitHutch.CreateBus(connectionString, reg =>
            {
                reg.Register<ISerializer>(_ => new SystemTextJsonSerializer());
            }));

            services.AddSingleton<IRabbitMqConnectionManager, RabbitMqConnectionManager>();
            services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
            services.AddSingleton<IRabbitMqSubscriber, RabbitMqSubscriber>();

            if (startup)
            {
                services.AddHostedService<MessagingStartupService>();
            }

            return services;
        }
    }
}
