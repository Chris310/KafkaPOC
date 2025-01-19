using Infrastructure.Shared.Messaging.DTO;
using Infrastructure.Shared.Messaging;

namespace MicroserviceConsumerA
{
    public class HistoryMessageHandler : IMessageHandler<HistoryMessageDTO>
    {
        private readonly ILogger<HistoryMessageHandler> _logger;

        public HistoryMessageHandler(ILogger<HistoryMessageHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleMessageAsync(HistoryMessageDTO message)
        {
            _logger.LogInformation($"Mando a procesar {message.Fecha.ToString()}");
            Console.WriteLine($"Mando a procesar {message.Fecha.ToString()}");

            await InsertHistoricoAsync(message);

            _logger.LogInformation("Terminó de procesar.");
        }

        public async Task InsertHistoricoAsync(HistoryMessageDTO message)
        {
            try
            {
                Console.WriteLine($"Proceso mensaje HistoryMessageDTO {message.Fecha.ToString()}");

                _logger.LogInformation($"Proceso mensaje HistoryMessageDTO { message.Fecha.ToString()}");
            }
            catch (Exception exConn)
            {
                _logger.LogError(exConn, "Error en InsertHistoricoAsync");
                throw;
            }
        }
    }
}
