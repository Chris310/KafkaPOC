using Infrastructure.Shared.Messaging.DTO;
using SharedKernel.Messaging;

namespace MessageHandler.History
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMessageConsumer<HistoryMessageDTO> _consumer;
        private readonly IBatchMessageHandler<HistoryMessageDTO> _handler;
        private readonly int _batchSize;

        public Worker(ILogger<Worker> logger, IMessageConsumer<HistoryMessageDTO> consumer, IBatchMessageHandler<HistoryMessageDTO> handler, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));

            _batchSize = configuration.GetValue<int>("BatchProcessing:BatchSize");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MessageHandler.History Worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var messages = new List<HistoryMessageDTO>();

                    for (int i = 0; i < _batchSize; i++)
                    {
                        var message = _consumer.Consume();
                        if (message == null)
                        {
                            // Significa que no hay más mensajes disponibles en este momento
                            break;
                        }
                        messages.Add(message);
                    }

                    if (messages.Count > 0)
                    {
                        _logger.LogInformation("Received {Count} messages, processing batch...", messages.Count);
                        await _handler.HandleBatchAsync(messages);
                        
                         _consumer.Commit();
                    }
                    else
                    {
                        // Si no hay mensajes, espero un poco antes de reintentar
                        await Task.Delay(500, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing a batch of messages.");
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
