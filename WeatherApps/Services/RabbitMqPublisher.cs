using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WeatherApps.Messaging;

namespace WeatherApps.Services
{
    public sealed class RabbitMqPublisher
    {
        private readonly RabbitMqSettings _settings;
        public RabbitMqPublisher(IOptions<RabbitMqSettings> options)
        {
            _settings = options.Value;
        }
        public async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message must be provided", nameof(message));

            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            // Async connection creation
            using var connection = await factory.CreateConnectionAsync(cancellationToken);

            // Async channel creation
            using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            // Synchronous queue declaration (7.2.0)
            //await channel.QueueDeclareAsync(
            //     queue: _settings.QueueName,
            //     durable: true,
            //     exclusive: false,
            //     autoDelete: false,
            //     arguments: null
            // );

            // Prepare message
            // Exchange declare
            await channel.ExchangeDeclareAsync(_settings.ExchangeName, ExchangeType.Direct, durable: true);
            await channel.QueueDeclareAsync(_settings.QueueName, durable: true, exclusive: false, autoDelete: false);
            // Bind queue to exchange
            await channel.QueueBindAsync(_settings.QueueName, _settings.ExchangeName, routingKey: _settings.QueueName);


            var body = Encoding.UTF8.GetBytes(message);

            // Synchronous publish
            await channel.BasicPublishAsync(
                exchange: _settings.ExchangeName,
                routingKey: _settings.QueueName,
                body: body,
                cancellationToken: cancellationToken
            );

            Console.WriteLine($"Message sent: {message}");
        }

    }
}