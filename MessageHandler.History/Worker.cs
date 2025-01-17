using Infrastructure.Shared.Messaging.DTO;
using SharedKernel.Messaging;

namespace MessageHandler.History
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMessageConsumer<HistoryMessageDTOv2> _consumer;
        private readonly IBatchMessageHandler<HistoryMessageDTOv2> _handler;
        private readonly int _batchSize;
        private readonly TimeSpan _consumeTimeout;

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
