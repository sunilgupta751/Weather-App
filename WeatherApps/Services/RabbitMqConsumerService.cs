using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WeatherApps.Messaging;

namespace WeatherApps.Services
{
    public sealed class RabbitMqConsumerService : BackgroundService
    {
        private readonly ILogger<RabbitMqConsumerService> _logger;
        private readonly RabbitMqSettings _settings;

        public RabbitMqConsumerService(ILogger<RabbitMqConsumerService> logger, IOptions<RabbitMqSettings> options)
        {
            _logger = logger;
            _settings = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Retry loop for connection
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var factory = new ConnectionFactory
                    {
                        HostName = _settings.HostName,
                        UserName = _settings.UserName,
                        Password = _settings.Password,
                        //DispatchConsumersAsync = true // async consumer support
                    };

                    await using var connection = await factory.CreateConnectionAsync(stoppingToken);
                    await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
                    await channel.ExchangeDeclareAsync(_settings.ExchangeName, ExchangeType.Direct, durable: true);
                    await channel.QueueDeclareAsync(_settings.QueueName, durable: true, exclusive: false, autoDelete: false);
                    // Bind queue to exchange
                    await channel.QueueBindAsync(_settings.QueueName, _settings.ExchangeName, routingKey: _settings.QueueName);

                    // Queue declaration
                    await channel.QueueDeclareAsync(
                        queue: _settings.QueueName,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                    var consumer = new AsyncEventingBasicConsumer(channel);
                    consumer.ReceivedAsync += async (model, ea) =>
                    {
                        try
                        {
                            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                            _logger.LogInformation("Message Received: {Message}", message);
                            await Task.Yield(); // placeholder for async processing
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing message");
                        }
                    };

                    // Start consuming
                    await channel.BasicConsumeAsync(
                        queue: _settings.QueueName,
                        autoAck: true,
                        consumer: consumer,
                        cancellationToken: stoppingToken
                    );

                    _logger.LogInformation("RabbitMQ consumer started");
                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "RabbitMQ connection failed. Retrying in 5 seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }
    }
}
