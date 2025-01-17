using Infrastructure.Shared.Messaging.DTO;
using SharedKernel.Messaging;
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMessageProducer<HistoryMessageDTOv2> _producer;

    public Worker(ILogger<Worker> logger, IMessageProducer<HistoryMessageDTOv2> producer)
    {
        _logger = logger;
        _producer = producer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("***** EMPEZANDO A MANDAR MENSAJES *****");
        Console.WriteLine("***** EMPEZANDO A MANDAR MENSAJES *****");

        Random random = new Random();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                HistoryMessageDTOv2 message = new HistoryMessageDTOv2();
                message.Fecha = DateTime.Now;
                message.InfoPublica = "Test Data1";
                message.InfoPrivada = "Test Data2";
                message.InfoSolicitud = "Test InfoSolicitud";
                message.InfoRespuesta = "Valor InfoRespuesta";

                await _producer.PublishAsync(message);

                _logger.LogInformation($"Mensaje enviado HistoryMessageDTOv2: {message.Fecha.ToString()}, ***** {message.InfoSolicitud}");
                Console.WriteLine($"Mensaje enviado HistoryMessageDTOv2: {message.Fecha.ToString()}, ***** {message.InfoSolicitud}");

                await Task.Delay(50, stoppingToken);
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

