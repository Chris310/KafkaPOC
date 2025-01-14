using Infrastructure.Shared.Messaging.DTO;
using SharedKernel.Messaging;

namespace MessageHandler.History
{
    public class HistoryMessageHandler : IMessageHandler<HistoryMessageDTO>
    {
        /// <summary>
        /// Procesa el mensaje recibido.
        /// </summary>
        /// <param name="message">El mensaje de tipo HistoryMessageDTO.</param>
        /// <returns>Una tarea asincrónica.</returns>
        public async Task HandleMessageAsync(HistoryMessageDTO message)
        {
            // Simulación de procesamiento (por ejemplo, guardar en base de datos)
            await Task.Run(() =>
            {
                Console.WriteLine($"Processing message: Data={message.Data}, Timestamp={message.Timestamp}");
            });
        }
    }
}
