using EasyNetQ;
using InventoryScanner.Messaging.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace InventoryScanner.Messaging.Infrastructure
{
    public class MessagingStartupService : IHostedService, IMessagingStartupService
    {
        private readonly IBus bus;
        private readonly List<RabbitMqInfrastructureTarget> targets;

        public MessagingStartupService(IBus bus, IOptions<List<RabbitMqInfrastructureTarget>> options)
        {
            this.bus = bus;
            this.targets = options.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var target in targets)
            {
                if (string.IsNullOrWhiteSpace(target.ExchangeName) || string.IsNullOrWhiteSpace(target.QueueName))
                    throw new InvalidOperationException("ExchangeName and QueueName must be provided for all RabbitMQ targets.");

                var exchange = await bus.Advanced.ExchangeDeclareAsync(target.ExchangeName, type: "fanout", durable: true, autoDelete: false, cancellationToken: cancellationToken);

                var queue = await bus.Advanced.QueueDeclareAsync(target.QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);

                await bus.Advanced.BindAsync(exchange, queue, routingKey: string.Empty, cancellationToken: cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
