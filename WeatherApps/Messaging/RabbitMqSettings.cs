using System;

namespace WeatherApps.Messaging
{
    public sealed class RabbitMqSettings
    {
        public string HostName { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
        public string QueueName { get; set; } = "";
        public string ExchangeName { get; set; } = "";
    }
}