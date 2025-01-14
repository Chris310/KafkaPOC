using Infrastructure.Shared.Messaging.DTO;
using SharedKernel.Messaging;

namespace MessageHandler.History
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMessageConsumer<HistoryMessageDTO> _consumer;
        private readonly IMessageHandler<HistoryMessageDTO> _handler;

        public Worker(ILogger<Worker> logger, IMessageConsumer<HistoryMessageDTO> consumer, IMessageHandler<HistoryMessageDTO> handler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MessageHandler.History Worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Consume mensaje del tópico
                    var message = _consumer.Consume();

                    if (message == null)
                    {
                        _logger.LogWarning("Received null message.");
                        continue;
                    }

                    _logger.LogInformation("Message received: {Message}", message.Data);

                    // Procesar el mensaje
                    await _handler.HandleMessageAsync(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing a message.");
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker is stopping...");

            try
            {
                _consumer.Dispose();
                _logger.LogInformation("Consumer disposed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while disposing the consumer.");
            }

            await base.StopAsync(stoppingToken);
        }
    }
}
