using System.Text.Json;

namespace Infrastructure.Messaging.Kafka.Serialization
{
    public class JsonMessageSerializer : IMessageSerializer
    {
        public string Serialize<T>(T message)
        {
            return JsonSerializer.Serialize(message);
        }

        public T Deserialize<T>(string message)
        {
            return JsonSerializer.Deserialize<T>(message)
                ?? throw new InvalidOperationException($"Unable to deserialize message to type {typeof(T)}.");
        }
    }
}
