using Confluent.Kafka;
using Infrastructure.Shared.Messaging.DTO;
using Polly;
using Infrastructure.Shared.Messaging;

namespace MessageHandler.History
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMessageConsumer<HistoryMessageDTOv2> _consumer;
        private readonly IBatchMessageHandler<HistoryMessageDTOv2> _handler;
        private readonly int _batchSize;
        private readonly TimeSpan _consumeTimeout;
        private readonly int _maxRetryAttempts = 3; // Máximo número de intentos
        private readonly int _retryDelayMs = 1000;  // Retraso entre intentos en ms

        public Worker(ILogger<Worker> logger, IMessageConsumer<HistoryMessageDTOv2> consumer, IBatchMessageHandler<HistoryMessageDTOv2> handler, IConfiguration configuration)
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
            _logger.LogInformation("***** EMPEZANDO A RECIBIR MENSAJES *****");
            Console.WriteLine("***** EMPEZANDO A RECIBIR MENSAJES *****");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var messages = new List<HistoryMessageDTOv2>();

                    for (int i = 0; i < _batchSize; i++)
                    {
                        var message = _consumer.Consume(_consumeTimeout);
                        if (message == null)
                        {
                            Console.WriteLine("No hay mensajes en este momento");
                            break;
                        }
                        messages.Add(message);
                    }

                    if (messages.Count > 0)
                    {
                        _logger.LogInformation($"*** {messages.Count.ToString()} mensajes recibidos, llamo al handler para procesar batch. ***");
                        Console.WriteLine($"*** {messages.Count.ToString()} mensajes recibidos, llamo al handler para procesar batch. ***");

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
                            await _handler.HandleBatchAsync(messages);

                            _consumer.Commit();
                        });
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
