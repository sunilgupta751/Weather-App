using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WeatherApps.Services;

namespace WeatherApps.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class PublishController : ControllerBase
    {
        private readonly RabbitMqPublisher _publisher;
        private readonly ILogger<PublishController> _logger;

        public PublishController(RabbitMqPublisher publisher, ILogger<PublishController> logger)
        {
            _publisher = publisher;
            _logger = logger;
        }

    
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] string message, CancellationToken cancellationToken)
        {
            await _publisher.SendMessageAsync(message, cancellationToken);
            _logger.LogInformation("Published message via HTTP input.");

            return Accepted(new { status = "published" });
        }
    }
}