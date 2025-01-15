using Infrastructure.Shared.Messaging.DTO;
using SharedKernel.Messaging;
using System;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMessageProducer<HistoryMessageDTO> _producer;

    public Worker(ILogger<Worker> logger, IMessageProducer<HistoryMessageDTO> producer)
    {
        _logger = logger;
        _producer = producer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker started, sending messages to topic: History.");

        Random random = new Random();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                HistoryMessageDTO message = new HistoryMessageDTO();
                message.Fecha = DateTime.Now;
                message.InfoPublica = $"Test Data1 {Guid.NewGuid()}";
                message.InfoPrivada = "Test Data2";
                message.InfoSolicitud = random.Next(0, 5) == 0 ? null : $"Test InfoSolicitud {Guid.NewGuid()}";
                message.InfoRespuesta = "InfoRespuestaaaaaaaaaa";

                await _producer.PublishAsync(message);

                _logger.LogInformation($"Message sent to topic: History. {message.InfoPublica}, {message.Fecha.ToString()},   {message.InfoSolicitud ?? "N/A"}");
                Console.WriteLine($"Message sent to topic: History. {message.InfoPublica}, {message.Fecha.ToString()},    {message.InfoSolicitud ?? "N/A"}");

                await Task.Delay(500, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Worker cancellation requested.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending a message.");
            }
        }

        _logger.LogInformation("Worker is stopping.");
    }
}

