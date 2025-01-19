using Infrastructure.Shared.Messaging.DTO;
using Polly;
using Infrastructure.Shared.Messaging;
using Confluent.Kafka;

namespace MicroserviceConsumerA
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMessageConsumer<HistoryMessageDTO> _consumer;
        private readonly IMessageHandler<HistoryMessageDTO> _handler;
        private readonly int _batchSize;
        private readonly TimeSpan _consumeTimeout;
        private readonly int _maxRetryAttempts = 3; // Máximo número de intentos
        private readonly int _retryDelayMs = 1000;  // Retraso entre intentos en ms

        public Worker(ILogger<Worker> logger, IMessageConsumer<HistoryMessageDTO> consumer, IMessageHandler<HistoryMessageDTO> handler, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));

            _batchSize = configuration.GetValue<int>("BatchProcessing:BatchSize");

            var timeoutMs = configuration.GetValue<int?>("ConsumerOptions:ConsumeTimeoutMs") ?? 1000;
            _consumeTimeout = TimeSpan.FromMilliseconds(timeoutMs);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("***** MicroserviceConsumerB - EMPEZANDO A RECIBIR MENSAJES *****");
            Console.WriteLine("***** MicroserviceConsumerB - EMPEZANDO A RECIBIR MENSAJES  *****");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var message = _consumer.Consume(_consumeTimeout);

                    if (message == null)
                    {
                        Console.WriteLine("No hay mensajes en este momento");
                    }
                    else 
                    {
                        // Configurar la política de reintento
                        var retryPolicy = Policy
                            .Handle<Exception>() // Manejar cualquier excepción
                            .WaitAndRetryAsync(
                                _maxRetryAttempts,
                                attempt => TimeSpan.FromMilliseconds(_retryDelayMs),
                                (exception, timeSpan, attempt, context) =>
                                {
                                    _logger.LogWarning($"Attempt {attempt} failed with error: {exception.Message}. Retrying in {timeSpan.TotalMilliseconds}ms...");
                                });

                        // Ejecutar el procesamiento con política de reintento
                        await retryPolicy.ExecuteAsync(async () =>
                        {
                            await _handler.HandleMessageAsync(message);
                        });
                    }

                    await Task.Delay(1000, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing a batch of messages.");

                    //*** Aca se puede manejar un mecanismo de Dead Letter Queue (DLQ), mandando los mensajes a otro topic ***
                    //await _producer.PublishAsync(messages, "history-dlq-topic");
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
