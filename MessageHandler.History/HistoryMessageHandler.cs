using Infrastructure.Shared.Messaging.DTO;
using SharedKernel.Messaging;

namespace MessageHandler.History
{
    public class HistoryMessageHandler : IBatchMessageHandler<HistoryMessageDTO>
    {
        public async Task HandleBatchAsync(IEnumerable<HistoryMessageDTO> messages)
        {
            foreach (var msg in messages)
            {
                Console.WriteLine($"Processing message in batch: Data={msg.Data}, Timestamp={msg.Timestamp}");
            }

            await Task.Delay(100); //simulo procesar, aca hacer tema de bulk insert en BD
        }
    }
}
