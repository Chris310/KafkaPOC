
namespace Infrastructure.Messaging.Kafka.Serialization
{
    public interface IMessageSerializer
    {
        string Serialize<T>(T message);
        T Deserialize<T>(string message);
    }
}
